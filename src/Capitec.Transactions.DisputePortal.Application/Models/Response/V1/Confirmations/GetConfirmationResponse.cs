namespace Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Confirmations
{
    /// <summary>The submission confirmation of a dispute.</summary>
    public sealed record GetConfirmationResponse
    {
        /// <summary>The external id of the dispute.</summary>
        public required Guid DisputeId { get; init; }

        /// <summary>Pending until the confirmation is recorded, then Recorded.</summary>
        public required string Status { get; init; }

        /// <summary>The confirmation message, or null while pending.</summary>
        public string? Message { get; init; }

        /// <summary>The time the confirmation was recorded, or null while pending.</summary>
        public DateTimeOffset? RecordedAt { get; init; }
    }
}
