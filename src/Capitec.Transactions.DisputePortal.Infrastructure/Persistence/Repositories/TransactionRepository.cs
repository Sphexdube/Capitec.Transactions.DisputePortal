using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Transactions;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Repositories
{
    public sealed class TransactionRepository(DbSet<TransactionRecord> transactions) : ITransactionRepository
    {
        public Task<long?> GetOwnedTransactionIdAsync(
            Guid externalId,
            long customerId,
            CancellationToken cancellationToken)
        {
            return transactions
                .AsNoTracking()
                .Where(transaction => transaction.ExternalId == externalId && transaction.CustomerId == customerId)
                .Select(transaction => (long?)transaction.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<TransactionSummary?> GetOwnedSummaryByExternalIdAsync(
            Guid externalId,
            long customerId,
            CancellationToken cancellationToken)
        {
            return transactions
                .AsNoTracking()
                .Where(transaction => transaction.ExternalId == externalId && transaction.CustomerId == customerId)
                .Select(transaction => new TransactionSummary(
                    transaction.Id,
                    transaction.ExternalId,
                    transaction.Merchant,
                    transaction.Description,
                    transaction.OccurredAt,
                    transaction.AmountMinor,
                    transaction.CurrencyCode,
                    transaction.State))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<TransactionSummary?> GetOwnedSummaryAsync(
            long transactionId,
            long customerId,
            CancellationToken cancellationToken)
        {
            return transactions
                .AsNoTracking()
                .Where(transaction => transaction.Id == transactionId && transaction.CustomerId == customerId)
                .Select(transaction => new TransactionSummary(
                    transaction.Id,
                    transaction.ExternalId,
                    transaction.Merchant,
                    transaction.Description,
                    transaction.OccurredAt,
                    transaction.AmountMinor,
                    transaction.CurrencyCode,
                    transaction.State))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TransactionSummary>> GetPageAsync(
            long customerId,
            DateTimeOffset? beforeOccurredAt,
            long? beforeId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            int limit,
            CancellationToken cancellationToken)
        {
            IQueryable<TransactionRecord> query = transactions
                .AsNoTracking()
                .Where(transaction => transaction.CustomerId == customerId);

            if (from.HasValue)
            {
                query = query.Where(transaction => transaction.OccurredAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(transaction => transaction.OccurredAt <= to.Value);
            }

            if (beforeOccurredAt.HasValue && beforeId.HasValue)
            {
                query = query.Where(transaction =>
                    transaction.OccurredAt < beforeOccurredAt.Value ||
                    (transaction.OccurredAt == beforeOccurredAt.Value && transaction.Id < beforeId.Value));
            }

            List<TransactionSummary> transactionsPage = await query
                .OrderByDescending(transaction => transaction.OccurredAt)
                .ThenByDescending(transaction => transaction.Id)
                .Take(limit)
                .Select(transaction => new TransactionSummary(
                    transaction.Id,
                    transaction.ExternalId,
                    transaction.Merchant,
                    transaction.Description,
                    transaction.OccurredAt,
                    transaction.AmountMinor,
                    transaction.CurrencyCode,
                    transaction.State))
                .ToListAsync(cancellationToken);

            return transactionsPage;
        }

        public async Task<IReadOnlyList<TransactionSummary>> GetOwnedSummariesAsync(
            long customerId,
            IReadOnlyCollection<long> transactionIds,
            CancellationToken cancellationToken)
        {
            List<TransactionSummary> summaries = await transactions
                .AsNoTracking()
                .Where(transaction => transaction.CustomerId == customerId && transactionIds.Contains(transaction.Id))
                .Select(transaction => new TransactionSummary(
                    transaction.Id,
                    transaction.ExternalId,
                    transaction.Merchant,
                    transaction.Description,
                    transaction.OccurredAt,
                    transaction.AmountMinor,
                    transaction.CurrencyCode,
                    transaction.State))
                .ToListAsync(cancellationToken);

            return summaries;
        }
    }
}