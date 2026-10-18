using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Context
{
    public sealed class DisputePortalDbContext(DbContextOptions<DisputePortalDbContext> options) : DbContext(options)
    {
        public required DbSet<CustomerRecord> Customers { get; set; }

        public required DbSet<TransactionRecord> Transactions { get; set; }

        public required DbSet<Dispute> Disputes { get; set; }

        public required DbSet<DisputeHistoryRecord> DisputeHistory { get; set; }

        public required DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }

        public required DbSet<OutboxMessageRecord> OutboxMessages { get; set; }

        public required DbSet<ConfirmationRecord> Confirmations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DisputePortalDbContext).Assembly);
        }
    }
}
