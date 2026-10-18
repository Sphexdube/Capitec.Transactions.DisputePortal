using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Microsoft.Extensions.Logging;

namespace Capitec.Transactions.DisputePortal.Application.Handlers.V1
{
    public sealed class GetDisputeHistoryHandler(
        ILogger<GetDisputeHistoryHandler> logger,
        IUnitOfWorkFactory<IUnitOfWork> unitOfWorkFactory)
        : IRequestHandler<GetDisputeHistoryRequest, IReadOnlyList<DisputeHistoryResponse>>
    {
        public async Task<IReadOnlyList<DisputeHistoryResponse>> HandleAsync(
            GetDisputeHistoryRequest request,
            CancellationToken cancellationToken = default)
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

                    IReadOnlyList<DisputeHistoryEntry> history = await unitOfWork.DisputeHistory.GetByDisputeIdAsync(
                        dispute.Id,
                        cancellationToken);

                    logger.LogDebug(InfoMessages.MethodCompleted);
                    return history
                        .Select(entry => new DisputeHistoryResponse
                        {
                            EventType = entry.EventType,
                            PreviousStatus = entry.PreviousStatus,
                            CurrentStatus = entry.CurrentStatus,
                            OccurredAt = entry.OccurredAt,
                            Actor = entry.Actor
                        })
                        .ToArray();
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
