namespace Capitec.Transactions.DisputePortal.Application.Paging
{
    public sealed record PageCursor(DateTimeOffset Position, long Id);
}
