using Capitec.Transactions.DisputePortal.Application.Paging;
using Capitec.Transactions.DisputePortal.Application.Validators.V1;
using Capitec.Transactions.DisputePortal.UnitTests.Builders;
using FluentValidation.Results;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Validators.V1
{
    public sealed class PagingRequestValidatorTests
    {
        private static readonly DateTimeOffset Earlier = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset Later = new(2026, 10, 2, 0, 0, 0, TimeSpan.Zero);

        private readonly GetDisputesRequestValidator _disputesValidator = new();
        private readonly GetTransactionsRequestValidator _transactionsValidator = new();

        [TestCase(1)]
        [TestCase(50)]
        public void Validate_WhenPageSizeIsInRange_ShouldHaveNoErrors(int pageSize)
        {
            string cursor = PageCursorCodec.Encode(new PageCursor(Earlier, 1));

            ValidationResult disputes = _disputesValidator.Validate(new GetDisputesRequestBuilder().WithPageSize(pageSize).WithCursor(cursor).Build());
            ValidationResult transactions = _transactionsValidator.Validate(
                new GetTransactionsRequestBuilder().WithPageSize(pageSize).WithCursor(cursor).WithRange(Earlier, Later).Build());

            Assert.That(disputes.IsValid, Is.True);
            Assert.That(transactions.IsValid, Is.True);
        }

        [TestCase(0)]
        [TestCase(51)]
        public void Validate_WhenPageSizeIsOutOfRange_ShouldReturnPageSizeError(int pageSize)
        {
            ValidationResult disputes = _disputesValidator.Validate(new GetDisputesRequestBuilder().WithPageSize(pageSize).Build());
            ValidationResult transactions = _transactionsValidator.Validate(new GetTransactionsRequestBuilder().WithPageSize(pageSize).Build());

            Assert.That(disputes.Errors.Single().PropertyName, Is.EqualTo("pageSize"));
            Assert.That(transactions.Errors.Single().PropertyName, Is.EqualTo("pageSize"));
        }

        [Test]
        public void Validate_WhenCursorIsInvalid_ShouldReturnCursorError()
        {
            ValidationResult disputes = _disputesValidator.Validate(new GetDisputesRequestBuilder().WithCursor("bad").Build());
            ValidationResult transactions = _transactionsValidator.Validate(new GetTransactionsRequestBuilder().WithCursor("bad").Build());

            Assert.That(disputes.Errors.Single().ErrorMessage, Is.EqualTo("The dispute cursor is invalid."));
            Assert.That(transactions.Errors.Single().ErrorMessage, Is.EqualTo("The transaction cursor is invalid."));
        }

        [Test]
        public void Validate_WhenCustomerIdIsNotPositive_ShouldReturnCustomerError()
        {
            ValidationResult disputes = _disputesValidator.Validate(new GetDisputesRequestBuilder().WithCustomerId(0).Build());
            ValidationResult transactions = _transactionsValidator.Validate(new GetTransactionsRequestBuilder().WithCustomerId(0).Build());

            Assert.That(disputes.Errors.Single().PropertyName, Is.EqualTo("customerId"));
            Assert.That(transactions.Errors.Single().PropertyName, Is.EqualTo("customerId"));
        }

        [Test]
        public void Validate_WhenEndIsBeforeStart_ShouldReturnRangeError()
        {
            ValidationResult result = _transactionsValidator.Validate(new GetTransactionsRequestBuilder().WithRange(Later, Earlier).Build());

            Assert.That(result.Errors.Single().PropertyName, Is.EqualTo("to"));
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void Validate_WhenOnlyOneRangeBoundIsSet_ShouldHaveNoErrors(bool hasFrom, bool hasTo)
        {
            ValidationResult result = _transactionsValidator.Validate(new GetTransactionsRequestBuilder()
                .WithRange(hasFrom ? Later : null, hasTo ? Earlier : null)
                .Build());

            Assert.That(result.IsValid, Is.True);
        }
    }
}
