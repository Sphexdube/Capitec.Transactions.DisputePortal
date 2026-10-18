namespace Capitec.Transactions.DisputePortal.Application.Models.Request.V1
{
    /// <summary>Gets one page of transactions for the signed-in customer, newest first.</summary>
    public sealed record GetTransactionsRequest
    {
        /// <summary>The id of the signed-in customer.</summary>
        public required long CustomerId { get; init; }

        /// <summary>The cursor from the previous page, or null for the first page.</summary>
        public string? Cursor { get; init; }

        /// <summary>The number of transactions in the page, from 1 to 50.</summary>
        public required int PageSize { get; init; }

        /// <summary>The optional earliest transaction time to include.</summary>
        public DateTimeOffset? From { get; init; }

        /// <summary>The optional latest transaction time to include.</summary>
        public DateTimeOffset? To { get; init; }
    }
}
