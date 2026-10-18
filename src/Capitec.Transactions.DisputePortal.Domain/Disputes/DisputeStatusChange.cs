namespace Capitec.Transactions.DisputePortal.Domain.Disputes
{
    public sealed record DisputeStatusChange(
        DisputeStatus PreviousStatus,
        DisputeStatus CurrentStatus,
        DateTimeOffset ChangedAt);
}
