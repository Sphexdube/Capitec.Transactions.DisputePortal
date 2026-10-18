using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Repositories
{
    public sealed class DisputeIdempotencyRepository(DbSet<IdempotencyRecord> idempotencyRecords) : IDisputeIdempotencyRepository
    {
        public Task<DisputeIdempotency?> GetAsync(
            long customerId,
            string operation,
            string key,
            CancellationToken cancellationToken)
        {
            return idempotencyRecords
                .AsNoTracking()
                .Where(record => record.CustomerId == customerId && record.Operation == operation && record.IdempotencyKey == key)
                .Select(record => new DisputeIdempotency(
                    record.CustomerId,
                    record.Operation,
                    record.IdempotencyKey,
                    record.RequestHash,
                    record.DisputeExternalId,
                    record.CreatedAt))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public void Add(DisputeIdempotency idempotency)
        {
            ArgumentNullException.ThrowIfNull(idempotency);

            idempotencyRecords.Add(new IdempotencyRecord(
                idempotency.CustomerId,
                idempotency.Operation,
                idempotency.Key,
                idempotency.RequestHash,
                idempotency.DisputeExternalId,
                idempotency.CreatedAt));
        }
    }
}
