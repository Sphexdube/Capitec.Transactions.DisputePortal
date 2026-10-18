using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Transactions;
using Capitec.Transactions.DisputePortal.Application.Paging;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Capitec.Transactions.DisputePortal.Domain.Transactions;
using Microsoft.Extensions.Logging;

namespace Capitec.Transactions.DisputePortal.Application.Handlers.V1
{
    public sealed class GetTransactionsHandler(
        ILogger<GetTransactionsHandler> logger,
        IUnitOfWorkFactory<IUnitOfWork> unitOfWorkFactory) : IRequestHandler<GetTransactionsRequest, GetTransactionsResponse>
    {
        public async Task<GetTransactionsResponse> HandleAsync(
            GetTransactionsRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            logger.LogDebug(InfoMessages.MethodStarted);

            try
            {
                PageCursor? cursor = PageCursorCodec.Decode(request.Cursor);

                using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
                {
                    IReadOnlyList<TransactionSummary> rows = await unitOfWork.Transactions.GetPageAsync(
                        request.CustomerId,
                        cursor?.Position,
                        cursor?.Id,
                        request.From?.ToUniversalTime(),
                        request.To?.ToUniversalTime(),
                        request.PageSize + 1,
                        cancellationToken);
                    TransactionSummary[] page = rows.Take(request.PageSize).ToArray();
                    string? nextCursor = rows.Count > request.PageSize
                        ? PageCursorCodec.Encode(new PageCursor(page[^1].OccurredAt, page[^1].Id))
                        : null;

                    logger.LogDebug(InfoMessages.MethodCompleted);
                    return new GetTransactionsResponse
                    {
                        Items = page.Select(TransactionSummaryResponse.From).ToArray(),
                        NextCursor = nextCursor,
                        PageSize = request.PageSize
                    };
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ErrorMessages.MethodFailed);
                throw;
            }
        }
    }
}
