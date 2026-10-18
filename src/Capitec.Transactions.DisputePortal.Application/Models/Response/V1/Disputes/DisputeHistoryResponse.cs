namespace Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes
{
    /// <summary>One change in the status history of a dispute.</summary>
    public sealed record DisputeHistoryResponse
    {
        /// <summary>The kind of change.</summary>
        public required string EventType { get; init; }

        /// <summary>The status before the change, or null for the submission.</summary>
        public string? PreviousStatus { get; init; }

        /// <summary>The status after the change.</summary>
        public required string CurrentStatus { get; init; }

        /// <summary>The time the change occurred.</summary>
        public required DateTimeOffset OccurredAt { get; init; }

        /// <summary>The party that made the change.</summary>
        public required string Actor { get; init; }
    }
}
