using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Transactions;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Capitec.Transactions.DisputePortal.Domain.Transactions;
using Microsoft.Extensions.Logging;

namespace Capitec.Transactions.DisputePortal.Application.Handlers.V1
{
    public sealed class GetDisputeHandler(
        ILogger<GetDisputeHandler> logger,
        IUnitOfWorkFactory<IUnitOfWork> unitOfWorkFactory) : IRequestHandler<GetDisputeRequest, GetDisputeResponse>
    {
        public async Task<GetDisputeResponse> HandleAsync(GetDisputeRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            logger.LogDebug(InfoMessages.MethodStarted);

            try
            {
                using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
                {
                    Dispute? dispute = await unitOfWork.Disputes.GetByExternalIdAsync(
                        request.DisputeId,
                        request.CustomerId,
                        cancellationToken);

                    if (dispute is null)
                    {
                        throw new DisputeNotFoundException(ErrorMessages.DisputeNotFound);
                    }

                    TransactionSummary? transaction = await unitOfWork.Transactions.GetOwnedSummaryAsync(
                        dispute.TransactionId,
                        request.CustomerId,
                        cancellationToken);

                    if (transaction is null)
                    {
                        throw new InvalidOperationException(ErrorMessages.DisputeMissingTransaction);
                    }

                    logger.LogDebug(InfoMessages.MethodCompleted);
                    return new GetDisputeResponse
                    {
                        DisputeId = dispute.ExternalId,
                        Transaction = TransactionSummaryResponse.From(transaction),
                        Reason = dispute.Reason,
                        Explanation = dispute.Explanation,
                        Status = dispute.Status,
                        CreatedAt = dispute.CreatedAt,
                        UpdatedAt = dispute.UpdatedAt
                    };
                }
            }
            catch (Exception exception) when (exception is not RequestRejectedException)
            {
                logger.LogError(exception, ErrorMessages.MethodFailed);
                throw;
            }
        }
    }
}
