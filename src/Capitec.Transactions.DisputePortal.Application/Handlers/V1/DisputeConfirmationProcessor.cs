using System.Text.Json;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Capitec.Transactions.DisputePortal.Domain.Outbox;
using Microsoft.Extensions.Logging;

namespace Capitec.Transactions.DisputePortal.Application.Handlers.V1
{
    public sealed class DisputeConfirmationProcessor(
        ILogger<DisputeConfirmationProcessor> logger,
        IUnitOfWorkFactory<IUnitOfWork> unitOfWorkFactory,
        TimeProvider timeProvider) : IDisputeConfirmationProcessor
    {
        public async Task<int> ProcessBatchAsync(int batchSize, CancellationToken cancellationToken)
        {
            logger.LogDebug(InfoMessages.MethodStarted);

            try
            {
                using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
                {
                    int processedCount = await unitOfWork.ExecuteInTransactionAsync(
                        token => ProcessAsync(unitOfWork, batchSize, token),
                        cancellationToken);

                    logger.LogDebug(InfoMessages.ConfirmationBatchProcessed, processedCount);
                    return processedCount;
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ErrorMessages.OutboxProcessingFailed);
                throw;
            }
        }

        private async Task<int> ProcessAsync(IUnitOfWork unitOfWork, int batchSize, CancellationToken cancellationToken)
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            IReadOnlyList<PendingOutboxMessage> messages = await unitOfWork.Outbox.LockDueAsync(now, batchSize, cancellationToken);
            List<Guid> confirmedEventIds = [];

            foreach (PendingOutboxMessage message in messages)
            {
                string? failureReason = await TryConfirmAsync(unitOfWork, message, now, cancellationToken);

                if (failureReason is null)
                {
                    confirmedEventIds.Add(message.EventId);
                    continue;
                }

                logger.LogWarning(WarningMessages.OutboxMessageRescheduled, message.EventId, failureReason);
                await unitOfWork.Outbox.RescheduleAsync(message.EventId, now.Add(OutboxLimits.RetryDelay), cancellationToken);
            }

            if (confirmedEventIds.Count > 0)
            {
                await unitOfWork.Outbox.DeleteAsync(confirmedEventIds, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return messages.Count;
        }

        private static async Task<string?> TryConfirmAsync(
            IUnitOfWork unitOfWork,
            PendingOutboxMessage message,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            if (message.EventType != OutboxEventTypes.DisputeSubmittedV1)
            {
                return ErrorMessages.OutboxUnsupportedEventType;
            }

            DisputeSubmittedEvent? submittedEvent = JsonSerializer.Deserialize<DisputeSubmittedEvent>(message.Payload);

            if (submittedEvent is null || submittedEvent.EventId != message.EventId)
            {
                return ErrorMessages.OutboxInvalidPayload;
            }

            long? disputeId = await unitOfWork.Disputes.GetIdByExternalIdAsync(submittedEvent.DisputeId, cancellationToken);

            if (disputeId is null)
            {
                return ErrorMessages.OutboxMissingDispute;
            }

            unitOfWork.Confirmations.Add(new DisputeConfirmation(
                disputeId.Value,
                message.EventId,
                InfoMessages.DisputeSubmissionReceived,
                now));

            return null;
        }
    }
}
