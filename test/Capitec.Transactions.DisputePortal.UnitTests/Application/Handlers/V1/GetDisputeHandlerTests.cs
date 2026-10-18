using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Application.Handlers.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Transactions;
using Capitec.Transactions.DisputePortal.UnitTests.Builders;
using Capitec.Transactions.DisputePortal.UnitTests.Setup;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Handlers.V1
{
    public sealed class GetDisputeHandlerTests
    {
        private const long CustomerId = 3;

        private UnitOfWorkSubstitute _unitOfWork = null!;
        private GetDisputeHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = new UnitOfWorkSubstitute();
            _handler = new GetDisputeHandler(NullLogger<GetDisputeHandler>.Instance, _unitOfWork.Factory);
        }

        [Test]
        public async Task HandleAsync_WhenDisputeIsOwned_ShouldReturnDisputeWithTransaction()
        {
            Dispute dispute = new DisputeBuilder().WithCustomerId(CustomerId).WithTransactionId(9).Build();
            TransactionSummary transaction = new TransactionSummaryBuilder().WithId(9).WithAmountMinor(125050).Build();
            _unitOfWork.Disputes.GetByExternalIdAsync(dispute.ExternalId, CustomerId, Arg.Any<CancellationToken>()).Returns(dispute);
            _unitOfWork.Transactions.GetOwnedSummaryAsync(9, CustomerId, Arg.Any<CancellationToken>()).Returns(transaction);

            GetDisputeResponse response = await _handler.HandleAsync(Request(dispute.ExternalId));

            Assert.That(response.DisputeId, Is.EqualTo(dispute.ExternalId));
            Assert.That(response.Reason, Is.EqualTo(dispute.Reason));
            Assert.That(response.Explanation, Is.EqualTo(dispute.Explanation));
            Assert.That(response.Status, Is.EqualTo(DisputeStatus.Submitted));
            Assert.That(response.UpdatedAt, Is.EqualTo(dispute.UpdatedAt));
            Assert.That(response.Transaction.TransactionId, Is.EqualTo(transaction.ExternalId));
            Assert.That(response.Transaction.AmountMinor, Is.EqualTo("125050"));
        }

        [Test]
        public void HandleAsync_WhenDisputeIsNotOwned_ShouldThrowDisputeNotFoundException()
        {
            Assert.ThrowsAsync<DisputeNotFoundException>(() => _handler.HandleAsync(Request(Guid.NewGuid())));
        }

        [Test]
        public void HandleAsync_WhenTransactionIsMissing_ShouldThrowInvalidOperationException()
        {
            Dispute dispute = new DisputeBuilder().WithCustomerId(CustomerId).Build();
            _unitOfWork.Disputes.GetByExternalIdAsync(dispute.ExternalId, CustomerId, Arg.Any<CancellationToken>()).Returns(dispute);

            Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(Request(dispute.ExternalId)));
        }

        [Test]
        public void HandleAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _handler.HandleAsync(null!));
        }

        private static GetDisputeRequest Request(Guid disputeId)
        {
            return new GetDisputeRequest { DisputeId = disputeId, CustomerId = CustomerId };
        }
    }
}
