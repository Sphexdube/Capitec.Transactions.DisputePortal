using Capitec.Transactions.DisputePortal.Application.Handlers.V1;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Outbox;
using Capitec.Transactions.DisputePortal.UnitTests.Builders;
using Capitec.Transactions.DisputePortal.UnitTests.Setup;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Handlers.V1
{
    public sealed class DisputeConfirmationProcessorTests
    {
        private const int BatchSize = 10;

        private UnitOfWorkSubstitute _unitOfWork = null!;
        private DisputeConfirmationProcessor _processor = null!;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = new UnitOfWorkSubstitute().PassThroughTransactions<int>();
            _processor = new DisputeConfirmationProcessor(NullLogger<DisputeConfirmationProcessor>.Instance, _unitOfWork.Factory, TimeProvider.System);
        }

        [Test]
        public async Task ProcessBatchAsync_WhenNoMessagesAreDue_ShouldReturnZeroWithoutWriting()
        {
            GivenDueMessages();

            int processed = await _processor.ProcessBatchAsync(BatchSize, CancellationToken.None);

            Assert.That(processed, Is.Zero);
            await _unitOfWork.Outbox.DidNotReceive().DeleteAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
            await _unitOfWork.UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ProcessBatchAsync_WhenSubmittedEventIsDue_ShouldRecordConfirmationAndDeleteMessage()
        {
            Guid disputeId = Guid.NewGuid();
            PendingOutboxMessage message = new PendingOutboxMessageBuilder().WithDisputeId(disputeId).Build();
            GivenDueMessages(message);
            _unitOfWork.Disputes.GetIdByExternalIdAsync(disputeId, Arg.Any<CancellationToken>()).Returns(77L);

            int processed = await _processor.ProcessBatchAsync(BatchSize, CancellationToken.None);

            Assert.That(processed, Is.EqualTo(1));
            _unitOfWork.Confirmations.Received(1).Add(Arg.Is<DisputeConfirmation>(confirmation =>
                confirmation.DisputeId == 77 && confirmation.OutboxEventId == message.EventId));
            await _unitOfWork.Outbox.Received(1).DeleteAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Single() == message.EventId),
                Arg.Any<CancellationToken>());
            await _unitOfWork.UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ProcessBatchAsync_WhenEventTypeIsUnsupported_ShouldRescheduleWithoutBlockingOthers()
        {
            Guid disputeId = Guid.NewGuid();
            PendingOutboxMessage poison = new PendingOutboxMessageBuilder().WithEventType("Other.v1").Build();
            PendingOutboxMessage valid = new PendingOutboxMessageBuilder().WithDisputeId(disputeId).Build();
            GivenDueMessages(poison, valid);
            _unitOfWork.Disputes.GetIdByExternalIdAsync(disputeId, Arg.Any<CancellationToken>()).Returns(5L);

            int processed = await _processor.ProcessBatchAsync(BatchSize, CancellationToken.None);

            Assert.That(processed, Is.EqualTo(2));
            await _unitOfWork.Outbox.Received(1).RescheduleAsync(poison.EventId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
            await _unitOfWork.Outbox.Received(1).DeleteAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Single() == valid.EventId),
                Arg.Any<CancellationToken>());
        }

        [TestCase("null")]
        [TestCase("{\"EventId\":\"00000000-0000-0000-0000-000000000001\"}")]
        public async Task ProcessBatchAsync_WhenPayloadIsInvalid_ShouldReschedule(string payload)
        {
            PendingOutboxMessage message = new PendingOutboxMessageBuilder().WithPayload(payload).Build();
            GivenDueMessages(message);

            await _processor.ProcessBatchAsync(BatchSize, CancellationToken.None);

            await _unitOfWork.Outbox.Received(1).RescheduleAsync(message.EventId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
            await _unitOfWork.Outbox.DidNotReceive().DeleteAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ProcessBatchAsync_WhenDisputeIsMissing_ShouldRescheduleOneRetryDelayLater()
        {
            PendingOutboxMessage message = new PendingOutboxMessageBuilder().Build();
            GivenDueMessages(message);
            DateTimeOffset before = DateTimeOffset.UtcNow;

            await _processor.ProcessBatchAsync(BatchSize, CancellationToken.None);

            _unitOfWork.Confirmations.DidNotReceive().Add(Arg.Any<DisputeConfirmation>());
            await _unitOfWork.Outbox.Received(1).RescheduleAsync(
                message.EventId,
                Arg.Is<DateTimeOffset>(next => next >= before.Add(OutboxLimits.RetryDelay)),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public void ProcessBatchAsync_WhenTransactionFails_ShouldRethrowTheFailure()
        {
            _unitOfWork.Outbox
                .LockDueAsync(Arg.Any<DateTimeOffset>(), BatchSize, Arg.Any<CancellationToken>())
                .Returns(Task.FromException<IReadOnlyList<PendingOutboxMessage>>(new TimeoutException()));

            Assert.ThrowsAsync<TimeoutException>(() => _processor.ProcessBatchAsync(BatchSize, CancellationToken.None));
        }

        private void GivenDueMessages(params PendingOutboxMessage[] messages)
        {
            _unitOfWork.Outbox
                .LockDueAsync(Arg.Any<DateTimeOffset>(), BatchSize, Arg.Any<CancellationToken>())
                .Returns(messages);
        }
    }
}
