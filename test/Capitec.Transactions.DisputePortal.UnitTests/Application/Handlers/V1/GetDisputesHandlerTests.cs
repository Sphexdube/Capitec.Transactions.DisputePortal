using Capitec.Transactions.DisputePortal.Application.Handlers.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Application.Paging;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Transactions;
using Capitec.Transactions.DisputePortal.UnitTests.Builders;
using Capitec.Transactions.DisputePortal.UnitTests.Setup;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Handlers.V1
{
    public sealed class GetDisputesHandlerTests
    {
        private UnitOfWorkSubstitute _unitOfWork = null!;
        private GetDisputesHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = new UnitOfWorkSubstitute();
            _handler = new GetDisputesHandler(NullLogger<GetDisputesHandler>.Instance, _unitOfWork.Factory);
        }

        [Test]
        public async Task HandleAsync_WhenMoreRowsExist_ShouldReturnPageAndNextCursor()
        {
            Dispute[] rows =
            [
                new DisputeBuilder().WithId(30).WithTransactionId(1).Build(),
                new DisputeBuilder().WithId(20).WithTransactionId(2).Build(),
                new DisputeBuilder().WithId(10).WithTransactionId(1).Build()
            ];
            _unitOfWork.Disputes.GetCustomerPageAsync(1, null, null, 3, Arg.Any<CancellationToken>()).Returns(rows);
            _unitOfWork.Transactions
                .GetOwnedSummariesAsync(1, Arg.Is<IReadOnlyCollection<long>>(ids => ids.Count == 2), Arg.Any<CancellationToken>())
                .Returns([new TransactionSummaryBuilder().WithId(1).Build(), new TransactionSummaryBuilder().WithId(2).Build()]);

            GetDisputesResponse response = await _handler.HandleAsync(new GetDisputesRequestBuilder().WithPageSize(2).Build());

            Assert.That(response.Items.Select(item => item.DisputeId), Is.EqualTo(new[] { rows[0].ExternalId, rows[1].ExternalId }));
            Assert.That(response.Items[0].Reason, Is.EqualTo(rows[0].Reason));
            Assert.That(response.Items[0].Status, Is.EqualTo(DisputeStatus.Submitted));
            Assert.That(response.Items[0].CreatedAt, Is.EqualTo(rows[0].CreatedAt));
            Assert.That(response.Items[0].UpdatedAt, Is.EqualTo(rows[0].UpdatedAt));
            Assert.That(response.PageSize, Is.EqualTo(2));
            Assert.That(PageCursorCodec.Decode(response.NextCursor), Is.EqualTo(new PageCursor(rows[1].CreatedAt, 20)));
        }

        [Test]
        public async Task HandleAsync_WhenCursorIsSupplied_ShouldReadBeforeTheCursor()
        {
            PageCursor cursor = new(new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero), 99);
            _unitOfWork.Disputes
                .GetCustomerPageAsync(1, cursor.Position, 99, 3, Arg.Any<CancellationToken>())
                .Returns(Array.Empty<Dispute>());
            _unitOfWork.Transactions
                .GetOwnedSummariesAsync(1, Arg.Any<IReadOnlyCollection<long>>(), Arg.Any<CancellationToken>())
                .Returns(Array.Empty<TransactionSummary>());

            GetDisputesResponse response = await _handler.HandleAsync(
                new GetDisputesRequestBuilder().WithCursor(PageCursorCodec.Encode(cursor)).Build());

            Assert.That(response.Items, Is.Empty);
            Assert.That(response.NextCursor, Is.Null);
        }

        [Test]
        public void HandleAsync_WhenTransactionIsMissing_ShouldThrowInvalidOperationException()
        {
            _unitOfWork.Disputes
                .GetCustomerPageAsync(1, null, null, 3, Arg.Any<CancellationToken>())
                .Returns([new DisputeBuilder().WithTransactionId(5).Build()]);
            _unitOfWork.Transactions
                .GetOwnedSummariesAsync(1, Arg.Any<IReadOnlyCollection<long>>(), Arg.Any<CancellationToken>())
                .Returns(Array.Empty<TransactionSummary>());

            Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(new GetDisputesRequestBuilder().Build()));
        }

        [Test]
        public void HandleAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _handler.HandleAsync(null!));
        }
    }
}
