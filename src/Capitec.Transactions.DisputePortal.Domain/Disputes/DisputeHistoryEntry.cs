namespace Capitec.Transactions.DisputePortal.Domain.Disputes
{
    public sealed record DisputeHistoryEntry(
        long DisputeId,
        string EventType,
        string? PreviousStatus,
        string CurrentStatus,
        DateTimeOffset OccurredAt,
        string Actor);
}
