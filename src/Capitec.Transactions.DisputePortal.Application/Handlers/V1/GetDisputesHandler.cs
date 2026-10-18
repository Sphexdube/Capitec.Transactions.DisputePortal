using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Transactions;
using Capitec.Transactions.DisputePortal.Application.Paging;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Capitec.Transactions.DisputePortal.Domain.Transactions;
using Microsoft.Extensions.Logging;

namespace Capitec.Transactions.DisputePortal.Application.Handlers.V1
{
    public sealed class GetDisputesHandler(
        ILogger<GetDisputesHandler> logger,
        IUnitOfWorkFactory<IUnitOfWork> unitOfWorkFactory) : IRequestHandler<GetDisputesRequest, GetDisputesResponse>
    {
        public async Task<GetDisputesResponse> HandleAsync(GetDisputesRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            logger.LogDebug(InfoMessages.MethodStarted);

            try
            {
                PageCursor? cursor = PageCursorCodec.Decode(request.Cursor);

                using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
                {
                    IReadOnlyList<Dispute> rows = await unitOfWork.Disputes.GetCustomerPageAsync(
                        request.CustomerId,
                        cursor?.Position,
                        cursor?.Id,
                        request.PageSize + 1,
                        cancellationToken);
                    Dispute[] page = rows.Take(request.PageSize).ToArray();
                    long[] transactionIds = page.Select(dispute => dispute.TransactionId).Distinct().ToArray();
                    IReadOnlyList<TransactionSummary> summaries = await unitOfWork.Transactions.GetOwnedSummariesAsync(
                        request.CustomerId,
                        transactionIds,
                        cancellationToken);
                    Dictionary<long, TransactionSummary> transactions = summaries.ToDictionary(transaction => transaction.Id);
                    string? nextCursor = rows.Count > request.PageSize
                        ? PageCursorCodec.Encode(new PageCursor(page[^1].CreatedAt, page[^1].Id))
                        : null;

                    logger.LogDebug(InfoMessages.MethodCompleted);
                    return new GetDisputesResponse
                    {
                        Items = page.Select(dispute => CreateListItem(dispute, transactions)).ToArray(),
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

        private static DisputeListItemResponse CreateListItem(
            Dispute dispute,
            IReadOnlyDictionary<long, TransactionSummary> transactions)
        {
            if (!transactions.TryGetValue(dispute.TransactionId, out TransactionSummary? transaction))
            {
                throw new InvalidOperationException(ErrorMessages.DisputeMissingTransaction);
            }

            return new DisputeListItemResponse
            {
                DisputeId = dispute.ExternalId,
                Transaction = TransactionSummaryResponse.From(transaction),
                Reason = dispute.Reason,
                Status = dispute.Status,
                CreatedAt = dispute.CreatedAt,
                UpdatedAt = dispute.UpdatedAt
            };
        }
    }
}
