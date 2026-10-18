namespace Capitec.Transactions.DisputePortal.Application.Models.Request.V1
{
    /// <summary>Gets one page of disputes for the signed-in customer, newest first.</summary>
    public sealed record GetDisputesRequest
    {
        /// <summary>The id of the signed-in customer.</summary>
        public required long CustomerId { get; init; }

        /// <summary>The cursor from the previous page, or null for the first page.</summary>
        public string? Cursor { get; init; }

        /// <summary>The number of disputes in the page, from 1 to 50.</summary>
        public required int PageSize { get; init; }
    }
}
