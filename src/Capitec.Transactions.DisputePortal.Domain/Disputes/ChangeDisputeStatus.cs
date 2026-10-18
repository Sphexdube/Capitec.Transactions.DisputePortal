namespace Capitec.Transactions.DisputePortal.Domain.Disputes
{
    public sealed record ChangeDisputeStatus
    {
        public required DisputeStatus Status { get; init; }

        public required DateTimeOffset ChangedAt { get; init; }
    }
}
