using Capitec.Transactions.DisputePortal.Domain.Customers;
using Capitec.Transactions.DisputePortal.Domain.Transactions;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Context;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Repositories;
using Capitec.Transactions.DisputePortal.IntegrationTests.Fixtures;

namespace Capitec.Transactions.DisputePortal.IntegrationTests.Infrastructure.Persistence.Repositories
{
    public sealed class CustomerAndTransactionRepositoryTests : DatabaseTestBase
    {
        private static readonly DateTimeOffset Day1 = new(2026, 9, 1, 8, 0, 0, TimeSpan.Zero);

        [Test]
        public async Task CustomerRepository_WhenCustomerExists_ShouldReturnSummaries()
        {
            CustomerRecord customer = await SeedCustomerAsync();

            using (DisputePortalDbContext context = CreateContext())
            {
                CustomerRepository repository = new(context.Customers);

                IReadOnlyList<CustomerSummary> all = await repository.GetAllAsync(CancellationToken.None);
                CustomerSummary? single = await repository.GetByIdAsync(customer.Id, CancellationToken.None);
                CustomerSummary? missing = await repository.GetByIdAsync(long.MaxValue, CancellationToken.None);

                Assert.That(all, Does.Contain(new CustomerSummary(customer.Id, customer.DisplayName)));
                Assert.That(single, Is.EqualTo(new CustomerSummary(customer.Id, customer.DisplayName)));
                Assert.That(missing, Is.Null);
            }
        }

        [Test]
        public async Task TransactionRepository_WhenTransactionIsOwned_ShouldReturnIdAndSummary()
        {
            CustomerRecord owner = await SeedCustomerAsync();
            CustomerRecord other = await SeedCustomerAsync();
            TransactionRecord transaction = await SeedTransactionAsync(owner.Id, Day1);

            using (DisputePortalDbContext context = CreateContext())
            {
                TransactionRepository repository = new(context.Transactions);

                Assert.That(await repository.GetOwnedTransactionIdAsync(transaction.ExternalId, owner.Id, CancellationToken.None), Is.EqualTo(transaction.Id));
                Assert.That(await repository.GetOwnedTransactionIdAsync(transaction.ExternalId, other.Id, CancellationToken.None), Is.Null);

                TransactionSummary? summary = await repository.GetOwnedSummaryAsync(transaction.Id, owner.Id, CancellationToken.None);
                Assert.That(summary!.ExternalId, Is.EqualTo(transaction.ExternalId));
                Assert.That(summary.AmountMinor, Is.EqualTo(transaction.AmountMinor));
                Assert.That(await repository.GetOwnedSummaryAsync(transaction.Id, other.Id, CancellationToken.None), Is.Null);
                Assert.That((await repository.GetOwnedSummaryByExternalIdAsync(transaction.ExternalId, owner.Id, CancellationToken.None))!.Id, Is.EqualTo(transaction.Id));
                Assert.That(await repository.GetOwnedSummaryByExternalIdAsync(transaction.ExternalId, other.Id, CancellationToken.None), Is.Null);

                IReadOnlyList<TransactionSummary> summaries = await repository.GetOwnedSummariesAsync(owner.Id, [transaction.Id], CancellationToken.None);
                Assert.That(summaries.Single().Id, Is.EqualTo(transaction.Id));
            }
        }

        [Test]
        public async Task TransactionRepository_WhenPaging_ShouldOrderNewestFirstAndApplyCursorAndRange()
        {
            CustomerRecord customer = await SeedCustomerAsync();
            TransactionRecord oldest = await SeedTransactionAsync(customer.Id, Day1);
            TransactionRecord middle = await SeedTransactionAsync(customer.Id, Day1.AddDays(1));
            TransactionRecord newest = await SeedTransactionAsync(customer.Id, Day1.AddDays(2));

            using (DisputePortalDbContext context = CreateContext())
            {
                TransactionRepository repository = new(context.Transactions);

                IReadOnlyList<TransactionSummary> firstPage = await repository.GetPageAsync(customer.Id, null, null, null, null, 2, CancellationToken.None);
                IReadOnlyList<TransactionSummary> nextPage = await repository.GetPageAsync(
                    customer.Id, middle.OccurredAt, middle.Id, null, null, 2, CancellationToken.None);
                IReadOnlyList<TransactionSummary> ranged = await repository.GetPageAsync(
                    customer.Id, null, null, Day1.AddHours(1), Day1.AddDays(1).AddHours(1), 10, CancellationToken.None);

                Assert.That(firstPage.Select(item => item.Id), Is.EqualTo(new[] { newest.Id, middle.Id }));
                Assert.That(nextPage.Select(item => item.Id), Is.EqualTo(new[] { oldest.Id }));
                Assert.That(ranged.Select(item => item.Id), Is.EqualTo(new[] { middle.Id }));
            }
        }
    }
}
