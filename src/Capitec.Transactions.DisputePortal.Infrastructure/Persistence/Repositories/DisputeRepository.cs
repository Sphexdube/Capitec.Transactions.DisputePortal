using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Repositories
{
    public sealed class DisputeRepository(DbSet<Dispute> disputes) : IDisputeRepository
    {
        public Task<Dispute?> GetByExternalIdAsync(Guid externalId, long customerId, CancellationToken cancellationToken)
        {
            return disputes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    dispute => dispute.ExternalId == externalId && dispute.CustomerId == customerId,
                    cancellationToken);
        }

        public Task<long?> GetIdByExternalIdAsync(Guid externalId, CancellationToken cancellationToken)
        {
            return disputes
                .AsNoTracking()
                .Where(dispute => dispute.ExternalId == externalId)
                .Select(dispute => (long?)dispute.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<bool> HasActiveForTransactionAsync(long customerId, long transactionId, CancellationToken cancellationToken)
        {
            return disputes
                .AsNoTracking()
                .AnyAsync(
                    dispute => dispute.CustomerId == customerId
                        && dispute.TransactionId == transactionId
                        && (dispute.Status == DisputeStatus.Submitted || dispute.Status == DisputeStatus.UnderReview),
                    cancellationToken);
        }

        public async Task<IReadOnlyList<Dispute>> GetCustomerPageAsync(
            long customerId,
            DateTimeOffset? beforeCreatedAt,
            long? beforeId,
            int limit,
            CancellationToken cancellationToken)
        {
            IQueryable<Dispute> query = disputes
                .AsNoTracking()
                .Where(dispute => dispute.CustomerId == customerId);

            if (beforeCreatedAt.HasValue && beforeId.HasValue)
            {
                query = query.Where(dispute =>
                    dispute.CreatedAt < beforeCreatedAt.Value
                    || (dispute.CreatedAt == beforeCreatedAt.Value && dispute.Id < beforeId.Value));
            }

            List<Dispute> page = await query
                .OrderByDescending(dispute => dispute.CreatedAt)
                .ThenByDescending(dispute => dispute.Id)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return page;
        }

        public void Add(Dispute dispute)
        {
            ArgumentNullException.ThrowIfNull(dispute);

            disputes.Add(dispute);
        }
    }
}
