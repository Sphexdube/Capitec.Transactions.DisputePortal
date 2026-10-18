using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Constants;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Repositories
{
    public sealed class DisputeHistoryRepository(DbSet<DisputeHistoryRecord> historyRecords) : IDisputeHistoryRepository
    {
        public void Add(DisputeHistoryEntry historyEntry)
        {
            ArgumentNullException.ThrowIfNull(historyEntry);

            historyRecords.Add(new DisputeHistoryRecord(
                historyEntry.DisputeId,
                historyEntry.EventType,
                historyEntry.PreviousStatus,
                historyEntry.CurrentStatus,
                historyEntry.OccurredAt,
                historyEntry.Actor));
        }

        public async Task<IReadOnlyList<DisputeHistoryEntry>> GetByDisputeIdAsync(long disputeId, CancellationToken cancellationToken)
        {
            List<DisputeHistoryEntry> entries = await historyRecords
                .AsNoTracking()
                .Where(record => record.DisputeId == disputeId)
                .OrderBy(record => record.OccurredAt)
                .ThenBy(record => EF.Property<long>(record, ShadowProperties.Id))
                .Select(record => new DisputeHistoryEntry(
                    record.DisputeId,
                    record.EventType,
                    record.PreviousStatus,
                    record.CurrentStatus,
                    record.OccurredAt,
                    record.Actor))
                .ToListAsync(cancellationToken);

            return entries;
        }
    }
}
