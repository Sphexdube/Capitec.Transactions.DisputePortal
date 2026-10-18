using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Confirmations;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Microsoft.Extensions.Logging;

namespace Capitec.Transactions.DisputePortal.Application.Handlers.V1
{
    public sealed class GetConfirmationHandler(
        ILogger<GetConfirmationHandler> logger,
        IUnitOfWorkFactory<IUnitOfWork> unitOfWorkFactory) : IRequestHandler<GetConfirmationRequest, GetConfirmationResponse>
    {
        public async Task<GetConfirmationResponse> HandleAsync(
            GetConfirmationRequest request,
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

                    DisputeConfirmation? confirmation = await unitOfWork.Confirmations.GetByDisputeIdAsync(
                        dispute.Id,
                        cancellationToken);

                    logger.LogDebug(InfoMessages.MethodCompleted);

                    if (confirmation is null)
                    {
                        return new GetConfirmationResponse
                        {
                            DisputeId = dispute.ExternalId,
                            Status = ConfirmationStatuses.Pending
                        };
                    }

                    return new GetConfirmationResponse
                    {
                        DisputeId = dispute.ExternalId,
                        Status = ConfirmationStatuses.Recorded,
                        Message = confirmation.Message,
                        RecordedAt = confirmation.CreatedAt
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
