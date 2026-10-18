using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using NSubstitute;

namespace Capitec.Transactions.DisputePortal.UnitTests.Setup
{
    public sealed class UnitOfWorkSubstitute
    {
        public UnitOfWorkSubstitute()
        {
            Factory.Create().Returns(UnitOfWork);
            UnitOfWork.Customers.Returns(Customers);
            UnitOfWork.Disputes.Returns(Disputes);
            UnitOfWork.Transactions.Returns(Transactions);
            UnitOfWork.Idempotency.Returns(Idempotency);
            UnitOfWork.DisputeHistory.Returns(DisputeHistory);
            UnitOfWork.Outbox.Returns(Outbox);
            UnitOfWork.Confirmations.Returns(Confirmations);
        }

        public IUnitOfWorkFactory<IUnitOfWork> Factory { get; } = Substitute.For<IUnitOfWorkFactory<IUnitOfWork>>();

        public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();

        public ICustomerRepository Customers { get; } = Substitute.For<ICustomerRepository>();

        public IDisputeRepository Disputes { get; } = Substitute.For<IDisputeRepository>();

        public ITransactionRepository Transactions { get; } = Substitute.For<ITransactionRepository>();

        public IDisputeIdempotencyRepository Idempotency { get; } = Substitute.For<IDisputeIdempotencyRepository>();

        public IDisputeHistoryRepository DisputeHistory { get; } = Substitute.For<IDisputeHistoryRepository>();

        public IOutboxRepository Outbox { get; } = Substitute.For<IOutboxRepository>();

        public IConfirmationRepository Confirmations { get; } = Substitute.For<IConfirmationRepository>();

        public UnitOfWorkSubstitute PassThroughTransactions<TResult>()
        {
            UnitOfWork
                .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task<TResult>>>(), Arg.Any<CancellationToken>())
                .Returns(call => call.Arg<Func<CancellationToken, Task<TResult>>>()(call.Arg<CancellationToken>()));

            return this;
        }
    }
}
