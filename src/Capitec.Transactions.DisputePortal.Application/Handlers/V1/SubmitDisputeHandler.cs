using System.Security.Cryptography;
using System.Text.Json;
using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Exceptions;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Capitec.Transactions.DisputePortal.Domain.Outbox;
using Microsoft.Extensions.Logging;

namespace Capitec.Transactions.DisputePortal.Application.Handlers.V1
{
    public sealed class SubmitDisputeHandler(
        ILogger<SubmitDisputeHandler> logger,
        IUnitOfWorkFactory<IUnitOfWork> unitOfWorkFactory,
        TimeProvider timeProvider) : IRequestHandler<SubmitDisputeRequest, SubmitDisputeResponse>
    {
        public async Task<SubmitDisputeResponse> HandleAsync(
            SubmitDisputeRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            logger.LogDebug(InfoMessages.MethodStarted);

            try
            {
                string reason = request.Reason.Trim();
                string? explanation = string.IsNullOrWhiteSpace(request.Explanation) ? null : request.Explanation.Trim();
                string requestHash = CreateRequestHash(reason, explanation);
                DateTimeOffset submittedAt = timeProvider.GetUtcNow();

                SubmitDisputeResponse response;

                try
                {
                    response = await SubmitInNewUnitOfWorkAsync(request, reason, explanation, requestHash, submittedAt, cancellationToken);
                }
                catch (UniqueConstraintException exception)
                {
                    logger.LogWarning(exception, WarningMessages.ConcurrentSubmissionRetried);
                    response = await SubmitInNewUnitOfWorkAsync(request, reason, explanation, requestHash, submittedAt, cancellationToken);
                }

                logger.LogInformation(InfoMessages.DisputeSubmitted, response.DisputeId);
                logger.LogDebug(InfoMessages.MethodCompleted);
                return response;
            }
            catch (Exception exception) when (exception is not RequestRejectedException)
            {
                logger.LogError(exception, ErrorMessages.MethodFailed);
                throw;
            }
        }

        private async Task<SubmitDisputeResponse> SubmitInNewUnitOfWorkAsync(
            SubmitDisputeRequest request,
            string reason,
            string? explanation,
            string requestHash,
            DateTimeOffset submittedAt,
            CancellationToken cancellationToken)
        {
            using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
            {
                return await unitOfWork.ExecuteInTransactionAsync(
                    token => SubmitAsync(unitOfWork, request, reason, explanation, requestHash, submittedAt, token),
                    cancellationToken);
            }
        }

        private static async Task<SubmitDisputeResponse> SubmitAsync(
            IUnitOfWork unitOfWork,
            SubmitDisputeRequest request,
            string reason,
            string? explanation,
            string requestHash,
            DateTimeOffset submittedAt,
            CancellationToken cancellationToken)
        {
            DisputeIdempotency? existingIdempotency = await unitOfWork.Idempotency.GetAsync(
                request.CustomerId,
                DisputeOperations.SubmitTransactionDispute,
                request.IdempotencyKey,
                cancellationToken);

            if (existingIdempotency is not null)
            {
                return await GetReplayResponseAsync(unitOfWork, existingIdempotency, requestHash, cancellationToken);
            }

            long? transactionId = await unitOfWork.Transactions.GetOwnedTransactionIdAsync(
                request.TransactionId,
                request.CustomerId,
                cancellationToken);

            if (transactionId is null)
            {
                throw new TransactionNotFoundException(ErrorMessages.TransactionNotFound);
            }

            bool activeDisputeExists = await unitOfWork.Disputes.HasActiveForTransactionAsync(
                request.CustomerId,
                transactionId.Value,
                cancellationToken);

            if (activeDisputeExists)
            {
                DisputeIdempotency? committedIdempotency = await unitOfWork.Idempotency.GetAsync(
                    request.CustomerId,
                    DisputeOperations.SubmitTransactionDispute,
                    request.IdempotencyKey,
                    cancellationToken);

                if (committedIdempotency is not null)
                {
                    return await GetReplayResponseAsync(unitOfWork, committedIdempotency, requestHash, cancellationToken);
                }

                throw new ActiveDisputeExistsException(ErrorMessages.ActiveDisputeExists);
            }

            Guid disputeExternalId = Guid.NewGuid();
            Dispute dispute = new(new SubmitDispute
            {
                ExternalId = disputeExternalId,
                CustomerId = request.CustomerId,
                TransactionId = transactionId.Value,
                Reason = reason,
                Explanation = explanation,
                SubmittedAt = submittedAt
            });

            unitOfWork.Disputes.Add(dispute);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            Guid eventId = Guid.NewGuid();
            DisputeSubmittedEvent submittedEvent = new(eventId, disputeExternalId, request.TransactionId, submittedAt);

            unitOfWork.DisputeHistory.Add(new DisputeHistoryEntry(
                dispute.Id,
                DisputeHistoryEventTypes.Submitted,
                null,
                nameof(DisputeStatus.Submitted),
                submittedAt,
                DisputeActors.Customer));
            unitOfWork.Idempotency.Add(new DisputeIdempotency(
                request.CustomerId,
                DisputeOperations.SubmitTransactionDispute,
                request.IdempotencyKey,
                requestHash,
                disputeExternalId,
                submittedAt));
            unitOfWork.Outbox.Add(new PendingOutboxMessage(
                eventId,
                OutboxEventTypes.DisputeSubmittedV1,
                JsonSerializer.Serialize(submittedEvent),
                submittedAt));

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new SubmitDisputeResponse
            {
                DisputeId = disputeExternalId,
                Status = dispute.Status,
                CreatedAt = dispute.CreatedAt
            };
        }

        private static async Task<SubmitDisputeResponse> GetReplayResponseAsync(
            IUnitOfWork unitOfWork,
            DisputeIdempotency existingIdempotency,
            string requestHash,
            CancellationToken cancellationToken)
        {
            if (!string.Equals(existingIdempotency.RequestHash, requestHash, StringComparison.Ordinal))
            {
                throw new IdempotencyConflictException(ErrorMessages.IdempotencyConflict);
            }

            Dispute? existingDispute = await unitOfWork.Disputes.GetByExternalIdAsync(
                existingIdempotency.DisputeExternalId,
                existingIdempotency.CustomerId,
                cancellationToken);

            if (existingDispute is null)
            {
                throw new InvalidOperationException(ErrorMessages.IdempotencyRecordMissingDispute);
            }

            return new SubmitDisputeResponse
            {
                DisputeId = existingDispute.ExternalId,
                Status = existingDispute.Status,
                CreatedAt = existingDispute.CreatedAt
            };
        }

        private static string CreateRequestHash(string reason, string? explanation)
        {
            byte[] requestContent = JsonSerializer.SerializeToUtf8Bytes(new IdempotencyPayload(reason, explanation));

            return Convert.ToHexString(SHA256.HashData(requestContent));
        }

        private sealed record IdempotencyPayload(string Reason, string? Explanation);
    }
}
