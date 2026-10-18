using System.Security.Cryptography;
using System.Text.Json;
using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Application.Handlers.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Exceptions;
using Capitec.Transactions.DisputePortal.Domain.Outbox;
using Capitec.Transactions.DisputePortal.UnitTests.Builders;
using Capitec.Transactions.DisputePortal.UnitTests.Setup;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Handlers.V1
{
    public sealed class SubmitDisputeHandlerTests
    {
        private const long TransactionId = 42;
        private const string Operation = "SubmitTransactionDispute";

        private UnitOfWorkSubstitute _unitOfWork = null!;
        private SubmitDisputeHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = new UnitOfWorkSubstitute().PassThroughTransactions<SubmitDisputeResponse>();
            _unitOfWork.Transactions
                .GetOwnedTransactionIdAsync(Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
                .Returns(TransactionId);
            _handler = new SubmitDisputeHandler(NullLogger<SubmitDisputeHandler>.Instance, _unitOfWork.Factory, TimeProvider.System);
        }

        [Test]
        public async Task HandleAsync_WhenRequestIsValid_ShouldPersistDisputeHistoryIdempotencyAndOutbox()
        {
            SubmitDisputeRequest request = new SubmitDisputeRequestBuilder().Build();

            SubmitDisputeResponse response = await _handler.HandleAsync(request);

            Assert.That(response.Status, Is.EqualTo(DisputeStatus.Submitted));
            Assert.That(response.DisputeId, Is.Not.EqualTo(Guid.Empty));
            _unitOfWork.Disputes.Received(1).Add(Arg.Is<Dispute>(dispute =>
                dispute.CustomerId == request.CustomerId
                && dispute.TransactionId == TransactionId
                && dispute.Reason == "Unrecognized transaction"
                && dispute.Explanation == "Not mine"));
            _unitOfWork.DisputeHistory.Received(1).Add(Arg.Is<DisputeHistoryEntry>(entry =>
                entry.CurrentStatus == nameof(DisputeStatus.Submitted) && entry.Actor == DisputeActors.Customer && entry.PreviousStatus == null));
            _unitOfWork.Idempotency.Received(1).Add(Arg.Is<DisputeIdempotency>(entry =>
                entry.CustomerId == request.CustomerId && entry.DisputeExternalId == response.DisputeId && entry.Key == request.IdempotencyKey));
            _unitOfWork.Outbox.Received(1).Add(Arg.Is<PendingOutboxMessage>(message =>
                message.EventType == OutboxEventTypes.DisputeSubmittedV1 && message.Payload.Contains(response.DisputeId.ToString())));
            await _unitOfWork.UnitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
            _unitOfWork.UnitOfWork.Received(1).Dispose();
        }

        [Test]
        public async Task HandleAsync_WhenExplanationIsBlank_ShouldStoreNoExplanation()
        {
            SubmitDisputeRequest request = new SubmitDisputeRequestBuilder().WithExplanation("   ").Build();

            await _handler.HandleAsync(request);

            _unitOfWork.Disputes.Received(1).Add(Arg.Is<Dispute>(dispute => dispute.Explanation == null));
        }

        [Test]
        public void HandleAsync_WhenTransactionIsNotOwnedByCustomer_ShouldThrowTransactionNotFoundException()
        {
            _unitOfWork.Transactions
                .GetOwnedTransactionIdAsync(Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
                .Returns((long?)null);

            Assert.ThrowsAsync<TransactionNotFoundException>(() => _handler.HandleAsync(new SubmitDisputeRequestBuilder().Build()));

            _unitOfWork.Disputes.DidNotReceive().Add(Arg.Any<Dispute>());
        }

        [Test]
        public void HandleAsync_WhenAnActiveDisputeExists_ShouldThrowActiveDisputeExistsException()
        {
            _unitOfWork.Disputes
                .HasActiveForTransactionAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
                .Returns(true);

            Assert.ThrowsAsync<ActiveDisputeExistsException>(() => _handler.HandleAsync(new SubmitDisputeRequestBuilder().Build()));

            _unitOfWork.Disputes.DidNotReceive().Add(Arg.Any<Dispute>());
        }

        [Test]
        public async Task HandleAsync_WhenSameKeyCommitsBetweenChecks_ShouldReturnOriginalDispute()
        {
            SubmitDisputeRequest request = new SubmitDisputeRequestBuilder().Build();
            Dispute original = new DisputeBuilder().WithCustomerId(request.CustomerId).Build();
            _unitOfWork.Idempotency
                .GetAsync(request.CustomerId, Operation, request.IdempotencyKey, Arg.Any<CancellationToken>())
                .Returns(
                    (DisputeIdempotency?)null,
                    new DisputeIdempotency(
                        request.CustomerId,
                        Operation,
                        request.IdempotencyKey,
                        CreateRequestHash("Unrecognized transaction", "Not mine"),
                        original.ExternalId,
                        DateTimeOffset.UnixEpoch));
            _unitOfWork.Disputes
                .HasActiveForTransactionAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
                .Returns(true);
            _unitOfWork.Disputes
                .GetByExternalIdAsync(original.ExternalId, request.CustomerId, Arg.Any<CancellationToken>())
                .Returns(original);

            SubmitDisputeResponse response = await _handler.HandleAsync(request);

            Assert.That(response.DisputeId, Is.EqualTo(original.ExternalId));
            _unitOfWork.Disputes.DidNotReceive().Add(Arg.Any<Dispute>());
        }

        [Test]
        public async Task HandleAsync_WhenIdempotencyKeyMatchesSamePayload_ShouldReturnOriginalDispute()
        {
            SubmitDisputeRequest request = new SubmitDisputeRequestBuilder().Build();
            Dispute original = new DisputeBuilder().WithCustomerId(request.CustomerId).Build();
            GivenExistingIdempotency(request, CreateRequestHash("Unrecognized transaction", "Not mine"), original.ExternalId);
            _unitOfWork.Disputes
                .GetByExternalIdAsync(original.ExternalId, request.CustomerId, Arg.Any<CancellationToken>())
                .Returns(original);

            SubmitDisputeResponse response = await _handler.HandleAsync(request);

            Assert.That(response.DisputeId, Is.EqualTo(original.ExternalId));
            Assert.That(response.CreatedAt, Is.EqualTo(original.CreatedAt));
            _unitOfWork.Disputes.DidNotReceive().Add(Arg.Any<Dispute>());
            await _unitOfWork.UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void HandleAsync_WhenIdempotencyKeyMatchesDifferentPayload_ShouldThrowIdempotencyConflictException()
        {
            SubmitDisputeRequest request = new SubmitDisputeRequestBuilder().Build();
            GivenExistingIdempotency(request, "different-request-hash", Guid.NewGuid());

            Assert.ThrowsAsync<IdempotencyConflictException>(() => _handler.HandleAsync(request));
        }

        [Test]
        public void HandleAsync_WhenIdempotentDisputeIsMissing_ShouldThrowInvalidOperationException()
        {
            SubmitDisputeRequest request = new SubmitDisputeRequestBuilder().Build();
            GivenExistingIdempotency(request, CreateRequestHash("Unrecognized transaction", "Not mine"), Guid.NewGuid());

            Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(request));
        }

        [Test]
        public async Task HandleAsync_WhenConcurrentRequestCommitsFirst_ShouldRetryOnceInNewUnitOfWork()
        {
            int attempts = 0;
            _unitOfWork.UnitOfWork
                .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task<SubmitDisputeResponse>>>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    attempts++;

                    if (attempts == 1)
                    {
                        throw new UniqueConstraintException("x", new TimeoutException());
                    }

                    return call.Arg<Func<CancellationToken, Task<SubmitDisputeResponse>>>()(call.Arg<CancellationToken>());
                });

            SubmitDisputeResponse response = await _handler.HandleAsync(new SubmitDisputeRequestBuilder().Build());

            Assert.That(response.Status, Is.EqualTo(DisputeStatus.Submitted));
            Assert.That(attempts, Is.EqualTo(2));
            _unitOfWork.Factory.Received(2).Create();
            _unitOfWork.UnitOfWork.Received(2).Dispose();
        }

        [Test]
        public void HandleAsync_WhenRetryAlsoConflicts_ShouldRethrowUniqueConstraintException()
        {
            _unitOfWork.UnitOfWork
                .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task<SubmitDisputeResponse>>>(), Arg.Any<CancellationToken>())
                .Throws(new UniqueConstraintException("x", new TimeoutException()));

            Assert.ThrowsAsync<UniqueConstraintException>(() => _handler.HandleAsync(new SubmitDisputeRequestBuilder().Build()));

            _unitOfWork.Factory.Received(2).Create();
        }

        [Test]
        public void HandleAsync_WhenUnitOfWorkFails_ShouldRethrowTheFailure()
        {
            _unitOfWork.Factory.Create().Throws(new TimeoutException());

            Assert.ThrowsAsync<TimeoutException>(() => _handler.HandleAsync(new SubmitDisputeRequestBuilder().Build()));
        }

        [Test]
        public void HandleAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _handler.HandleAsync(null!));
        }

        private void GivenExistingIdempotency(SubmitDisputeRequest request, string requestHash, Guid disputeExternalId)
        {
            _unitOfWork.Idempotency
                .GetAsync(request.CustomerId, Operation, request.IdempotencyKey, Arg.Any<CancellationToken>())
                .Returns(new DisputeIdempotency(request.CustomerId, Operation, request.IdempotencyKey, requestHash, disputeExternalId, DateTimeOffset.UnixEpoch));
        }

        private static string CreateRequestHash(string reason, string? explanation)
        {
            byte[] requestContent = JsonSerializer.SerializeToUtf8Bytes(new { Reason = reason, Explanation = explanation });

            return Convert.ToHexString(SHA256.HashData(requestContent));
        }
    }
}
