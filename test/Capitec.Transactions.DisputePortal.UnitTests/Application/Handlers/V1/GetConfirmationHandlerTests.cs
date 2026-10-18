using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Application.Handlers.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Confirmations;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.UnitTests.Builders;
using Capitec.Transactions.DisputePortal.UnitTests.Setup;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Handlers.V1
{
    public sealed class GetConfirmationHandlerTests
    {
        private UnitOfWorkSubstitute _unitOfWork = null!;
        private GetConfirmationHandler _handler = null!;
        private Dispute _dispute = null!;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = new UnitOfWorkSubstitute();
            _handler = new GetConfirmationHandler(NullLogger<GetConfirmationHandler>.Instance, _unitOfWork.Factory);
            _dispute = new DisputeBuilder().WithId(4).Build();
            _unitOfWork.Disputes.GetByExternalIdAsync(_dispute.ExternalId, 1, Arg.Any<CancellationToken>()).Returns(_dispute);
        }

        [Test]
        public async Task HandleAsync_WhenConfirmationIsNotRecorded_ShouldReturnPending()
        {
            GetConfirmationResponse response = await _handler.HandleAsync(Request(_dispute.ExternalId));

            Assert.That(response.DisputeId, Is.EqualTo(_dispute.ExternalId));
            Assert.That(response.Status, Is.EqualTo(ConfirmationStatuses.Pending));
            Assert.That(response.Message, Is.Null);
            Assert.That(response.RecordedAt, Is.Null);
        }

        [Test]
        public async Task HandleAsync_WhenConfirmationIsRecorded_ShouldReturnRecordedMessage()
        {
            DisputeConfirmation confirmation = new(4, Guid.NewGuid(), "Received.", DateTimeOffset.UnixEpoch);
            _unitOfWork.Confirmations.GetByDisputeIdAsync(4, Arg.Any<CancellationToken>()).Returns(confirmation);

            GetConfirmationResponse response = await _handler.HandleAsync(Request(_dispute.ExternalId));

            Assert.That(response.Status, Is.EqualTo(ConfirmationStatuses.Recorded));
            Assert.That(response.Message, Is.EqualTo("Received."));
            Assert.That(response.RecordedAt, Is.EqualTo(DateTimeOffset.UnixEpoch));
        }

        [Test]
        public void HandleAsync_WhenDisputeIsNotOwned_ShouldThrowDisputeNotFoundException()
        {
            Assert.ThrowsAsync<DisputeNotFoundException>(() => _handler.HandleAsync(Request(Guid.NewGuid())));
        }

        [Test]
        public void HandleAsync_WhenUnitOfWorkFails_ShouldRethrowTheFailure()
        {
            _unitOfWork.Factory.Create().Throws(new TimeoutException());

            Assert.ThrowsAsync<TimeoutException>(() => _handler.HandleAsync(Request(Guid.NewGuid())));
        }

        [Test]
        public void HandleAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _handler.HandleAsync(null!));
        }

        private static GetConfirmationRequest Request(Guid disputeId)
        {
            return new GetConfirmationRequest { DisputeId = disputeId, CustomerId = 1 };
        }
    }
}
