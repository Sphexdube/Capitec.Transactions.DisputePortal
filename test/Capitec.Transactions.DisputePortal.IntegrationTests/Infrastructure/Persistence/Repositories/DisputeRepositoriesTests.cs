using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Outbox;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Context;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Repositories;
using Capitec.Transactions.DisputePortal.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Transactions.DisputePortal.IntegrationTests.Infrastructure.Persistence.Repositories
{
    public sealed class DisputeRepositoriesTests : DatabaseTestBase
    {
        private static readonly DateTimeOffset SubmittedAt = new(2026, 9, 10, 8, 0, 0, TimeSpan.Zero);

        [Test]
        public async Task DisputeRepository_WhenDisputesExist_ShouldFindPageAndDetectActive()
        {
            CustomerRecord customer = await SeedCustomerAsync();
            TransactionRecord first = await SeedTransactionAsync(customer.Id, SubmittedAt);
            TransactionRecord second = await SeedTransactionAsync(customer.Id, SubmittedAt);
            Dispute older = await SeedDisputeAsync(customer.Id, first.Id, SubmittedAt);
            Dispute newer = await SeedDisputeAsync(customer.Id, second.Id, SubmittedAt.AddHours(1));

            using (DisputePortalDbContext context = CreateContext())
            {
                DisputeRepository repository = new(context.Disputes);

                Assert.That((await repository.GetByExternalIdAsync(older.ExternalId, customer.Id, CancellationToken.None))!.Id, Is.EqualTo(older.Id));
                Assert.That(await repository.GetByExternalIdAsync(older.ExternalId, customer.Id + 1000, CancellationToken.None), Is.Null);
                Assert.That(await repository.GetIdByExternalIdAsync(newer.ExternalId, CancellationToken.None), Is.EqualTo(newer.Id));
                Assert.That(await repository.GetIdByExternalIdAsync(Guid.NewGuid(), CancellationToken.None), Is.Null);
                Assert.That(await repository.HasActiveForTransactionAsync(customer.Id, first.Id, CancellationToken.None), Is.True);
                Assert.That(await repository.HasActiveForTransactionAsync(customer.Id, long.MaxValue, CancellationToken.None), Is.False);

                IReadOnlyList<Dispute> page = await repository.GetCustomerPageAsync(customer.Id, null, null, 1, CancellationToken.None);
                IReadOnlyList<Dispute> next = await repository.GetCustomerPageAsync(customer.Id, newer.CreatedAt, newer.Id, 5, CancellationToken.None);

                Assert.That(page.Single().Id, Is.EqualTo(newer.Id));
                Assert.That(next.Single().Id, Is.EqualTo(older.Id));
            }
        }

        [Test]
        public async Task HistoryIdempotencyAndConfirmationRepositories_WhenEntriesAreAdded_ShouldReadThemBack()
        {
            CustomerRecord customer = await SeedCustomerAsync();
            TransactionRecord transaction = await SeedTransactionAsync(customer.Id, SubmittedAt);
            Dispute dispute = await SeedDisputeAsync(customer.Id, transaction.Id, SubmittedAt);
            Guid eventId = Guid.NewGuid();

            using (DisputePortalDbContext context = CreateContext())
            {
                new DisputeHistoryRepository(context.DisputeHistory).Add(
                    new DisputeHistoryEntry(dispute.Id, "Submitted", null, "Submitted", SubmittedAt, "Customer"));
                new DisputeIdempotencyRepository(context.IdempotencyRecords).Add(
                    new DisputeIdempotency(customer.Id, "Op", "key-1", "hash", dispute.ExternalId, SubmittedAt));
                new ConfirmationRepository(context.Confirmations).Add(
                    new DisputeConfirmation(dispute.Id, eventId, "Received.", SubmittedAt));
                await context.SaveChangesAsync();
            }

            using (DisputePortalDbContext context = CreateContext())
            {
                IReadOnlyList<DisputeHistoryEntry> history = await new DisputeHistoryRepository(context.DisputeHistory)
                    .GetByDisputeIdAsync(dispute.Id, CancellationToken.None);
                DisputeIdempotency? idempotency = await new DisputeIdempotencyRepository(context.IdempotencyRecords)
                    .GetAsync(customer.Id, "Op", "key-1", CancellationToken.None);
                DisputeConfirmation? confirmation = await new ConfirmationRepository(context.Confirmations)
                    .GetByDisputeIdAsync(dispute.Id, CancellationToken.None);

                Assert.That(history.Single(), Is.EqualTo(new DisputeHistoryEntry(dispute.Id, "Submitted", null, "Submitted", SubmittedAt, "Customer")));
                Assert.That(idempotency!.RequestHash, Is.EqualTo("hash"));
                Assert.That(idempotency.Key, Is.EqualTo("key-1"));
                Assert.That(confirmation!.OutboxEventId, Is.EqualTo(eventId));
            }
        }

        [Test]
        public async Task OutboxRepository_WhenMessagesAreDue_ShouldLockOnlyDueMessagesAndDeleteThem()
        {
            DateTimeOffset now = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
            PendingOutboxMessage due = new(Guid.NewGuid(), "Test.v1", "{}", now.AddMinutes(-1));
            PendingOutboxMessage future = new(Guid.NewGuid(), "Test.v1", "{}", now.AddYears(5));

            using (DisputePortalDbContext context = CreateContext())
            {
                OutboxRepository repository = new(context.OutboxMessages);
                repository.Add(due);
                repository.Add(future);
                await context.SaveChangesAsync();
            }

            using (DisputePortalDbContext context = CreateContext())
            {
                OutboxRepository repository = new(context.OutboxMessages);

                IReadOnlyList<PendingOutboxMessage> locked = await repository.LockDueAsync(now, 1000, CancellationToken.None);
                int deleted = await repository.DeleteAsync([due.EventId, future.EventId], CancellationToken.None);

                Assert.That(locked.Select(message => message.EventId), Does.Contain(due.EventId).And.Not.Contain(future.EventId));
                Assert.That(locked.Single(message => message.EventId == due.EventId).EventType, Is.EqualTo("Test.v1"));
                Assert.That(deleted, Is.EqualTo(2));
            }
        }

        [Test]
        public async Task OutboxRepository_WhenMessageIsRescheduled_ShouldDelayItAndCountTheAttempt()
        {
            DateTimeOffset now = new(2031, 1, 1, 0, 0, 0, TimeSpan.Zero);
            PendingOutboxMessage message = new(Guid.NewGuid(), "Test.v1", "{}", now.AddMinutes(-1));

            using (DisputePortalDbContext context = CreateContext())
            {
                new OutboxRepository(context.OutboxMessages).Add(message);
                await context.SaveChangesAsync();
            }

            using (DisputePortalDbContext context = CreateContext())
            {
                OutboxRepository repository = new(context.OutboxMessages);

                int updated = await repository.RescheduleAsync(message.EventId, now.AddMinutes(5), CancellationToken.None);
                IReadOnlyList<PendingOutboxMessage> due = await repository.LockDueAsync(now, 1000, CancellationToken.None);
                int attempts = await context.OutboxMessages
                    .Where(record => record.EventId == message.EventId)
                    .Select(record => EF.Property<int>(record, "Attempts"))
                    .SingleAsync();
                await repository.DeleteAsync([message.EventId], CancellationToken.None);

                Assert.That(updated, Is.EqualTo(1));
                Assert.That(due.Select(item => item.EventId), Does.Not.Contain(message.EventId));
                Assert.That(attempts, Is.EqualTo(1));
            }
        }

        [Test]
        public void Repositories_WhenArgumentsAreNull_ShouldThrowArgumentNullException()
        {
            using (DisputePortalDbContext context = CreateContext())
            {
                Assert.Throws<ArgumentNullException>(() => new DisputeRepository(context.Disputes).Add(null!));
                Assert.Throws<ArgumentNullException>(() => new DisputeHistoryRepository(context.DisputeHistory).Add(null!));
                Assert.Throws<ArgumentNullException>(() => new DisputeIdempotencyRepository(context.IdempotencyRecords).Add(null!));
                Assert.Throws<ArgumentNullException>(() => new ConfirmationRepository(context.Confirmations).Add(null!));
                Assert.Throws<ArgumentNullException>(() => new OutboxRepository(context.OutboxMessages).Add(null!));
                Assert.ThrowsAsync<ArgumentNullException>(() => new OutboxRepository(context.OutboxMessages).DeleteAsync(null!, CancellationToken.None));
            }
        }
    }
}
