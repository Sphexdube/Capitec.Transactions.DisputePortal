using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Application.Handlers.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.UnitTests.Builders;
using Capitec.Transactions.DisputePortal.UnitTests.Setup;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Handlers.V1
{
    public sealed class GetDisputeHistoryHandlerTests
    {
        private UnitOfWorkSubstitute _unitOfWork = null!;
        private GetDisputeHistoryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = new UnitOfWorkSubstitute();
            _handler = new GetDisputeHistoryHandler(NullLogger<GetDisputeHistoryHandler>.Instance, _unitOfWork.Factory);
        }

        [Test]
        public async Task HandleAsync_WhenDisputeIsOwned_ShouldReturnHistoryInOrder()
        {
            Dispute dispute = new DisputeBuilder().WithId(8).Build();
            DisputeHistoryEntry entry = new(8, "Submitted", null, "Submitted", DateTimeOffset.UnixEpoch, "Customer");
            _unitOfWork.Disputes.GetByExternalIdAsync(dispute.ExternalId, 1, Arg.Any<CancellationToken>()).Returns(dispute);
            _unitOfWork.DisputeHistory.GetByDisputeIdAsync(8, Arg.Any<CancellationToken>()).Returns([entry]);

            IReadOnlyList<DisputeHistoryResponse> history = await _handler.HandleAsync(
                new GetDisputeHistoryRequest { DisputeId = dispute.ExternalId, CustomerId = 1 });

            Assert.That(history, Has.Count.EqualTo(1));
            Assert.That(history[0].EventType, Is.EqualTo(entry.EventType));
            Assert.That(history[0].PreviousStatus, Is.Null);
            Assert.That(history[0].CurrentStatus, Is.EqualTo(entry.CurrentStatus));
            Assert.That(history[0].OccurredAt, Is.EqualTo(entry.OccurredAt));
            Assert.That(history[0].Actor, Is.EqualTo(entry.Actor));
        }

        [Test]
        public void HandleAsync_WhenDisputeIsNotOwned_ShouldThrowDisputeNotFoundException()
        {
            Assert.ThrowsAsync<DisputeNotFoundException>(() => _handler.HandleAsync(
                new GetDisputeHistoryRequest { DisputeId = Guid.NewGuid(), CustomerId = 1 }));
        }

        [Test]
        public void HandleAsync_WhenUnitOfWorkFails_ShouldRethrowTheFailure()
        {
            _unitOfWork.Factory.Create().Throws(new TimeoutException());

            Assert.ThrowsAsync<TimeoutException>(() => _handler.HandleAsync(
                new GetDisputeHistoryRequest { DisputeId = Guid.NewGuid(), CustomerId = 1 }));
        }

        [Test]
        public void HandleAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _handler.HandleAsync(null!));
        }
    }
}
