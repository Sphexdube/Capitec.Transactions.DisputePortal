using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Transactions;
using Capitec.Transactions.DisputePortal.Domain.Disputes;

namespace Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes
{
    /// <summary>A customer dispute in a dispute page.</summary>
    public sealed record DisputeListItemResponse
    {
        /// <summary>The external id of the dispute.</summary>
        public required Guid DisputeId { get; init; }

        /// <summary>The disputed transaction.</summary>
        public required TransactionSummaryResponse Transaction { get; init; }

        /// <summary>The reason the customer disputes the transaction.</summary>
        public required string Reason { get; init; }

        /// <summary>The dispute status.</summary>
        public required DisputeStatus Status { get; init; }

        /// <summary>The time the dispute was submitted.</summary>
        public required DateTimeOffset CreatedAt { get; init; }

        /// <summary>The time the dispute last changed.</summary>
        public required DateTimeOffset UpdatedAt { get; init; }
    }
}
