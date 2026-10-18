using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Transactions;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Capitec.Transactions.DisputePortal.Domain.Transactions;
using Microsoft.Extensions.Logging;

namespace Capitec.Transactions.DisputePortal.Application.Handlers.V1
{
    public sealed class GetTransactionHandler(
        ILogger<GetTransactionHandler> logger,
        IUnitOfWorkFactory<IUnitOfWork> unitOfWorkFactory) : IRequestHandler<GetTransactionRequest, TransactionSummaryResponse>
    {
        public async Task<TransactionSummaryResponse> HandleAsync(
            GetTransactionRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            logger.LogDebug(InfoMessages.MethodStarted);

            try
            {
                using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
                {
                    TransactionSummary? transaction = await unitOfWork.Transactions.GetOwnedSummaryByExternalIdAsync(
                        request.TransactionId,
                        request.CustomerId,
                        cancellationToken);

                    if (transaction is null)
                    {
                        throw new TransactionNotFoundException(ErrorMessages.TransactionNotFound);
                    }

                    logger.LogDebug(InfoMessages.MethodCompleted);
                    return TransactionSummaryResponse.From(transaction);
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
