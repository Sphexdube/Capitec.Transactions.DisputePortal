using Capitec.Transactions.DisputePortal.Application.Handlers.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Transactions;
using Capitec.Transactions.DisputePortal.Application.Paging;
using Capitec.Transactions.DisputePortal.Domain.Transactions;
using Capitec.Transactions.DisputePortal.UnitTests.Builders;
using Capitec.Transactions.DisputePortal.UnitTests.Setup;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Handlers.V1
{
    public sealed class GetTransactionsHandlerTests
    {
        private UnitOfWorkSubstitute _unitOfWork = null!;
        private GetTransactionsHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = new UnitOfWorkSubstitute();
            _handler = new GetTransactionsHandler(NullLogger<GetTransactionsHandler>.Instance, _unitOfWork.Factory);
        }

        [Test]
        public async Task HandleAsync_WhenMoreRowsExist_ShouldReturnPageAndNextCursor()
        {
            TransactionSummary[] rows =
            [
                new TransactionSummaryBuilder().WithId(3).Build(),
                new TransactionSummaryBuilder().WithId(2).Build(),
                new TransactionSummaryBuilder().WithId(1).Build()
            ];
            _unitOfWork.Transactions.GetPageAsync(1, null, null, null, null, 3, Arg.Any<CancellationToken>()).Returns(rows);

            GetTransactionsResponse response = await _handler.HandleAsync(new GetTransactionsRequestBuilder().WithPageSize(2).Build());

            Assert.That(response.Items, Has.Count.EqualTo(2));
            Assert.That(response.Items[0].Merchant, Is.EqualTo("Example Market"));
            Assert.That(response.Items[0].Description, Is.EqualTo("Card purchase"));
            Assert.That(response.Items[0].OccurredAt, Is.EqualTo(rows[0].OccurredAt));
            Assert.That(response.Items[0].CurrencyCode, Is.EqualTo("ZAR"));
            Assert.That(response.Items[0].State, Is.EqualTo("Completed"));
            Assert.That(response.PageSize, Is.EqualTo(2));
            Assert.That(PageCursorCodec.Decode(response.NextCursor), Is.EqualTo(new PageCursor(rows[1].OccurredAt, 2)));
        }

        [Test]
        public async Task HandleAsync_WhenRangeHasOffsets_ShouldQueryInUtc()
        {
            DateTimeOffset from = new(2026, 10, 1, 2, 0, 0, TimeSpan.FromHours(2));
            DateTimeOffset to = new(2026, 10, 2, 2, 0, 0, TimeSpan.FromHours(2));
            PageCursor cursor = new(new DateTimeOffset(2026, 10, 2, 0, 0, 0, TimeSpan.Zero), 7);
            _unitOfWork.Transactions
                .GetPageAsync(1, cursor.Position, 7, Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), 3, Arg.Any<CancellationToken>())
                .Returns(Array.Empty<TransactionSummary>());

            GetTransactionsResponse response = await _handler.HandleAsync(new GetTransactionsRequestBuilder()
                .WithCursor(PageCursorCodec.Encode(cursor))
                .WithRange(from, to)
                .Build());

            Assert.That(response.NextCursor, Is.Null);
            await _unitOfWork.Transactions.Received(1).GetPageAsync(
                1,
                cursor.Position,
                7,
                Arg.Is<DateTimeOffset?>(value => value!.Value.Offset == TimeSpan.Zero && value.Value == from),
                Arg.Is<DateTimeOffset?>(value => value!.Value.Offset == TimeSpan.Zero && value.Value == to),
                3,
                Arg.Any<CancellationToken>());
        }

        [Test]
        public void HandleAsync_WhenRepositoryFails_ShouldRethrowTheFailure()
        {
            _unitOfWork.Transactions
                .GetPageAsync(Arg.Any<long>(), Arg.Any<DateTimeOffset?>(), Arg.Any<long?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new TimeoutException());

            Assert.ThrowsAsync<TimeoutException>(() => _handler.HandleAsync(new GetTransactionsRequestBuilder().Build()));
        }

        [Test]
        public void HandleAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _handler.HandleAsync(null!));
        }
    }
}
