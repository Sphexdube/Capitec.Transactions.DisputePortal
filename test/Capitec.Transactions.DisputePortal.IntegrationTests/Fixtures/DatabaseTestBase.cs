using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Context;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Capitec.Transactions.DisputePortal.IntegrationTests.Builders;

namespace Capitec.Transactions.DisputePortal.IntegrationTests.Fixtures
{
    public abstract class DatabaseTestBase
    {
        protected static DisputePortalDbContext CreateContext()
        {
            return PostgresDatabaseFixture.ContextFactory.CreateDbContext();
        }

        protected static async Task<CustomerRecord> SeedCustomerAsync()
        {
            using (DisputePortalDbContext context = CreateContext())
            {
                CustomerRecord customer = new CustomerRecordBuilder().Build();
                context.Customers.Add(customer);
                await context.SaveChangesAsync();

                return customer;
            }
        }

        protected static async Task<TransactionRecord> SeedTransactionAsync(long customerId, DateTimeOffset occurredAt)
        {
            using (DisputePortalDbContext context = CreateContext())
            {
                TransactionRecord transaction = new TransactionRecordBuilder().WithCustomerId(customerId).WithOccurredAt(occurredAt).Build();
                context.Transactions.Add(transaction);
                await context.SaveChangesAsync();

                return transaction;
            }
        }

        protected static async Task<Dispute> SeedDisputeAsync(long customerId, long transactionId, DateTimeOffset submittedAt)
        {
            using (DisputePortalDbContext context = CreateContext())
            {
                Dispute dispute = new(new SubmitDispute
                {
                    ExternalId = Guid.NewGuid(),
                    CustomerId = customerId,
                    TransactionId = transactionId,
                    Reason = "Seeded reason",
                    SubmittedAt = submittedAt
                });
                context.Disputes.Add(dispute);
                await context.SaveChangesAsync();

                return dispute;
            }
        }
    }
}
