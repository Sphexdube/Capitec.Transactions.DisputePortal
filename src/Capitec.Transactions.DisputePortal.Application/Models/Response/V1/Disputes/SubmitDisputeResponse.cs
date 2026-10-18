using Capitec.Transactions.DisputePortal.Domain.Disputes;

namespace Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes
{
    /// <summary>The accepted dispute.</summary>
    public sealed record SubmitDisputeResponse
    {
        /// <summary>The external id of the dispute.</summary>
        public required Guid DisputeId { get; init; }

        /// <summary>The dispute status.</summary>
        public required DisputeStatus Status { get; init; }

        /// <summary>The time the dispute was submitted.</summary>
        public required DateTimeOffset CreatedAt { get; init; }
    }
}
