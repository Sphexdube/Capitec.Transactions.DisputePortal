namespace Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes
{
    /// <summary>One page of customer disputes, newest first.</summary>
    public sealed record GetDisputesResponse
    {
        /// <summary>The disputes in the page.</summary>
        public required IReadOnlyList<DisputeListItemResponse> Items { get; init; }

        /// <summary>The cursor for the next page, or null when no more pages exist.</summary>
        public string? NextCursor { get; init; }

        /// <summary>The requested page size.</summary>
        public required int PageSize { get; init; }
    }
}
