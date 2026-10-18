using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Outbox;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Constants;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Repositories
{
    public sealed class OutboxRepository(DbSet<OutboxMessageRecord> outboxMessages) : IOutboxRepository
    {
        public void Add(PendingOutboxMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            outboxMessages.Add(new OutboxMessageRecord(message.EventId, message.EventType, message.Payload, message.CreatedAt));
        }

        public async Task<IReadOnlyList<PendingOutboxMessage>> LockDueAsync(
            DateTimeOffset dueAt,
            int batchSize,
            CancellationToken cancellationToken)
        {
            List<OutboxMessageRecord> records = await outboxMessages
                .FromSql($"""
                    SELECT * FROM "tblOutboxMessage"
                    WHERE "NextAttemptAt" <= {dueAt}
                    ORDER BY "Id"
                    LIMIT {batchSize}
                    FOR UPDATE SKIP LOCKED
                    """)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return records
                .Select(record => new PendingOutboxMessage(record.EventId, record.EventType, record.Payload, record.CreatedAt))
                .ToArray();
        }

        public Task<int> RescheduleAsync(Guid eventId, DateTimeOffset nextAttemptAt, CancellationToken cancellationToken)
        {
            return outboxMessages
                .Where(message => message.EventId == eventId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(message => message.NextAttemptAt, nextAttemptAt)
                        .SetProperty(
                            message => EF.Property<int>(message, ShadowProperties.Attempts),
                            message => EF.Property<int>(message, ShadowProperties.Attempts) + 1),
                    cancellationToken);
        }

        public Task<int> DeleteAsync(IReadOnlyCollection<Guid> eventIds, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(eventIds);

            return outboxMessages
                .Where(message => eventIds.Contains(message.EventId))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
