using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Exceptions;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Context;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Capitec.Transactions.DisputePortal.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Transactions.DisputePortal.IntegrationTests.Infrastructure.Persistence.UnitOfWork
{
    public sealed class UnitOfWorkTests : DatabaseTestBase
    {
        private static readonly DateTimeOffset SubmittedAt = new(2026, 9, 20, 8, 0, 0, TimeSpan.Zero);

        [Test]
        public async Task ExecuteInTransactionAsync_WhenOperationSucceeds_ShouldCommitChanges()
        {
            CustomerRecord customer = await SeedCustomerAsync();
            TransactionRecord transaction = await SeedTransactionAsync(customer.Id, SubmittedAt);
            Dispute dispute = NewDispute(customer.Id, transaction.Id);

            using (IUnitOfWork unitOfWork = PostgresDatabaseFixture.UnitOfWorkFactory.Create())
            {
                int saved = await unitOfWork.ExecuteInTransactionAsync(async token =>
                {
                    unitOfWork.Disputes.Add(dispute);
                    return await unitOfWork.SaveChangesAsync(token);
                });

                Assert.That(saved, Is.EqualTo(1));
            }

            Assert.That(await CountDisputesAsync(dispute.ExternalId), Is.EqualTo(1));
        }

        [Test]
        public async Task ExecuteInTransactionAsync_WhenOperationThrows_ShouldRollBackAndRethrow()
        {
            CustomerRecord customer = await SeedCustomerAsync();
            TransactionRecord transaction = await SeedTransactionAsync(customer.Id, SubmittedAt);
            Dispute dispute = NewDispute(customer.Id, transaction.Id);

            using (IUnitOfWork unitOfWork = PostgresDatabaseFixture.UnitOfWorkFactory.Create())
            {
                Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.ExecuteInTransactionAsync<int>(async token =>
                {
                    unitOfWork.Disputes.Add(dispute);
                    await unitOfWork.SaveChangesAsync(token);
                    throw new InvalidOperationException("rollback");
                }));
            }

            Assert.That(await CountDisputesAsync(dispute.ExternalId), Is.Zero);
        }

        [Test]
        public void ExecuteInTransactionAsync_WhenSaveViolatesConstraint_ShouldThrowSaveContextException()
        {
            using (IUnitOfWork unitOfWork = PostgresDatabaseFixture.UnitOfWorkFactory.Create())
            {
                Assert.ThrowsAsync<SaveContextException>(() => unitOfWork.ExecuteInTransactionAsync(token =>
                {
                    unitOfWork.Disputes.Add(NewDispute(long.MaxValue, long.MaxValue));
                    return unitOfWork.SaveChangesAsync(token);
                }));
            }
        }

        [Test]
        public async Task ExecuteInTransactionAsync_WhenSecondActiveDisputeIsSaved_ShouldThrowUniqueConstraintException()
        {
            CustomerRecord customer = await SeedCustomerAsync();
            TransactionRecord transaction = await SeedTransactionAsync(customer.Id, SubmittedAt);
            await SaveDisputeAsync(NewDispute(customer.Id, transaction.Id));

            using (IUnitOfWork unitOfWork = PostgresDatabaseFixture.UnitOfWorkFactory.Create())
            {
                Assert.ThrowsAsync<UniqueConstraintException>(() => unitOfWork.ExecuteInTransactionAsync(token =>
                {
                    unitOfWork.Disputes.Add(NewDispute(customer.Id, transaction.Id));
                    return unitOfWork.SaveChangesAsync(token);
                }));
            }
        }

        [Test]
        public async Task SaveChangesAsync_WhenSecondActiveDisputeIsSavedOutsideTransaction_ShouldThrowUniqueConstraintException()
        {
            CustomerRecord customer = await SeedCustomerAsync();
            TransactionRecord transaction = await SeedTransactionAsync(customer.Id, SubmittedAt);
            await SaveDisputeAsync(NewDispute(customer.Id, transaction.Id));

            using (IUnitOfWork unitOfWork = PostgresDatabaseFixture.UnitOfWorkFactory.Create())
            {
                unitOfWork.Disputes.Add(NewDispute(customer.Id, transaction.Id));

                Assert.ThrowsAsync<UniqueConstraintException>(() => unitOfWork.SaveChangesAsync());
            }
        }

        [Test]
        public void SaveChangesAsync_WhenSaveViolatesConstraintOutsideTransaction_ShouldThrowSaveContextException()
        {
            using (IUnitOfWork unitOfWork = PostgresDatabaseFixture.UnitOfWorkFactory.Create())
            {
                unitOfWork.Disputes.Add(NewDispute(long.MaxValue, long.MaxValue));

                Assert.ThrowsAsync<SaveContextException>(() => unitOfWork.SaveChangesAsync());
            }
        }

        [Test]
        public async Task SaveChangesAsync_WhenNothingChanged_ShouldReturnZero()
        {
            using (IUnitOfWork unitOfWork = PostgresDatabaseFixture.UnitOfWorkFactory.Create())
            {
                Assert.That(await unitOfWork.SaveChangesAsync(), Is.Zero);
            }
        }

        [Test]
        public void ExecuteInTransactionAsync_WhenOperationIsNull_ShouldThrowArgumentNullException()
        {
            using (IUnitOfWork unitOfWork = PostgresDatabaseFixture.UnitOfWorkFactory.Create())
            {
                Assert.ThrowsAsync<ArgumentNullException>(() => unitOfWork.ExecuteInTransactionAsync<int>(null!));
            }
        }

        [Test]
        public async Task LocalSeedScripts_WhenApplied_ShouldCreateDemoCustomersAndTransactions()
        {
            using (DisputePortalDbContext context = CreateContext())
            {
                int demoCustomers = await context.Customers.CountAsync(customer => customer.DisplayName == "Alex Example" || customer.DisplayName == "Sam Sample");
                int demoTransactions = await context.Transactions.CountAsync(transaction => transaction.Description == "Synthetic card transaction");

                Assert.That(demoCustomers, Is.EqualTo(2));
                Assert.That(demoTransactions, Is.EqualTo(46));
            }
        }

        private static async Task SaveDisputeAsync(Dispute dispute)
        {
            using (IUnitOfWork unitOfWork = PostgresDatabaseFixture.UnitOfWorkFactory.Create())
            {
                unitOfWork.Disputes.Add(dispute);
                await unitOfWork.SaveChangesAsync();
            }
        }

        private static Dispute NewDispute(long customerId, long transactionId)
        {
            return new Dispute(new SubmitDispute
            {
                ExternalId = Guid.NewGuid(),
                CustomerId = customerId,
                TransactionId = transactionId,
                Reason = "Unit of work",
                SubmittedAt = SubmittedAt
            });
        }

        private static async Task<int> CountDisputesAsync(Guid externalId)
        {
            using (DisputePortalDbContext context = CreateContext())
            {
                return await context.Disputes.CountAsync(dispute => dispute.ExternalId == externalId);
            }
        }
    }
}
