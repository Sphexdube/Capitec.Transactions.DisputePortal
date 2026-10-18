namespace Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Transactions
{
    /// <summary>One page of customer transactions, newest first.</summary>
    public sealed record GetTransactionsResponse
    {
        /// <summary>The transactions in the page.</summary>
        public required IReadOnlyList<TransactionSummaryResponse> Items { get; init; }

        /// <summary>The cursor for the next page, or null when no more pages exist.</summary>
        public string? NextCursor { get; init; }

        /// <summary>The requested page size.</summary>
        public required int PageSize { get; init; }
    }
}
