using System.Data.Common;
using Capitec.Transactions.DisputePortal.Domain.Exceptions;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Context;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.UnitOfWork
{
    public sealed class UnitOfWork(DisputePortalDbContext context) : IUnitOfWork
    {
        public ICustomerRepository Customers { get; } = new CustomerRepository(context.Customers);

        public IDisputeRepository Disputes { get; } = new DisputeRepository(context.Disputes);

        public ITransactionRepository Transactions { get; } = new TransactionRepository(context.Transactions);

        public IDisputeIdempotencyRepository Idempotency { get; } = new DisputeIdempotencyRepository(context.IdempotencyRecords);

        public IDisputeHistoryRepository DisputeHistory { get; } = new DisputeHistoryRepository(context.DisputeHistory);

        public IOutboxRepository Outbox { get; } = new OutboxRepository(context.OutboxMessages);

        public IConfirmationRepository Confirmations { get; } = new ConfirmationRepository(context.Confirmations);

        public async Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);

            try
            {
                IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

                return await strategy.ExecuteAsync(
                    async token =>
                    {
                        context.ChangeTracker.Clear();

                        using (IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(token))
                        {
                            TResult result = await operation(token);
                            await transaction.CommitAsync(token);

                            return result;
                        }
                    },
                    cancellationToken);
            }
            catch (Exception exception) when (IsUniqueViolation(exception))
            {
                throw new UniqueConstraintException(ErrorMessages.UniqueConstraintViolated, exception);
            }
            catch (Exception exception) when (IsPersistenceFailure(exception))
            {
                throw new SaveContextException(ErrorMessages.SaveContextFailed, exception);
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (context.Database.CurrentTransaction is not null)
            {
                return await context.SaveChangesAsync(cancellationToken);
            }

            try
            {
                return await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception) when (IsUniqueViolation(exception))
            {
                throw new UniqueConstraintException(ErrorMessages.UniqueConstraintViolated, exception);
            }
            catch (Exception exception) when (IsPersistenceFailure(exception))
            {
                throw new SaveContextException(ErrorMessages.SaveContextFailed, exception);
            }
        }

        public void Dispose()
        {
            context.ChangeTracker.Clear();
            context.Dispose();
        }

        private static bool IsUniqueViolation(Exception exception)
        {
            return exception is DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } };
        }

        private static bool IsPersistenceFailure(Exception exception)
        {
            return exception is DbUpdateException or DbException or RetryLimitExceededException;
        }
    }
}
