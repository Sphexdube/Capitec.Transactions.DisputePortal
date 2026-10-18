namespace Capitec.Transactions.DisputePortal.Domain.Disputes
{
    public sealed record SubmitDispute
    {
        public required Guid ExternalId { get; init; }

        public required long CustomerId { get; init; }

        public required long TransactionId { get; init; }

        public required string Reason { get; init; }

        public string? Explanation { get; init; }

        public required DateTimeOffset SubmittedAt { get; init; }
    }
}
