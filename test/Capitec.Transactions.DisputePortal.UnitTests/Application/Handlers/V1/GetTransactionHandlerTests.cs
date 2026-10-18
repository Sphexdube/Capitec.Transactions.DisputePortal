using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Application.Handlers.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Transactions;
using Capitec.Transactions.DisputePortal.Domain.Transactions;
using Capitec.Transactions.DisputePortal.UnitTests.Builders;
using Capitec.Transactions.DisputePortal.UnitTests.Setup;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Handlers.V1
{
    public sealed class GetTransactionHandlerTests
    {
        private UnitOfWorkSubstitute _unitOfWork = null!;
        private GetTransactionHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = new UnitOfWorkSubstitute();
            _handler = new GetTransactionHandler(NullLogger<GetTransactionHandler>.Instance, _unitOfWork.Factory);
        }

        [Test]
        public async Task HandleAsync_WhenTransactionIsOwned_ShouldReturnSummary()
        {
            TransactionSummary transaction = new TransactionSummaryBuilder().Build();
            _unitOfWork.Transactions
                .GetOwnedSummaryByExternalIdAsync(transaction.ExternalId, 4, Arg.Any<CancellationToken>())
                .Returns(transaction);

            TransactionSummaryResponse response = await _handler.HandleAsync(
                new GetTransactionRequest { TransactionId = transaction.ExternalId, CustomerId = 4 });

            Assert.That(response.TransactionId, Is.EqualTo(transaction.ExternalId));
        }

        [Test]
        public void HandleAsync_WhenTransactionIsNotOwned_ShouldThrowTransactionNotFoundException()
        {
            Assert.ThrowsAsync<TransactionNotFoundException>(() => _handler.HandleAsync(
                new GetTransactionRequest { TransactionId = Guid.NewGuid(), CustomerId = 4 }));
        }

        [Test]
        public void HandleAsync_WhenUnitOfWorkFails_ShouldRethrowTheFailure()
        {
            _unitOfWork.Factory.Create().Throws(new TimeoutException());

            Assert.ThrowsAsync<TimeoutException>(() => _handler.HandleAsync(
                new GetTransactionRequest { TransactionId = Guid.NewGuid(), CustomerId = 4 }));
        }

        [Test]
        public void HandleAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _handler.HandleAsync(null!));
        }
    }
}
