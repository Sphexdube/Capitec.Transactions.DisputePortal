using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Validators.V1;
using Capitec.Transactions.DisputePortal.UnitTests.Builders;
using FluentValidation.Results;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Validators.V1
{
    public sealed class SubmitDisputeRequestValidatorTests
    {
        private readonly SubmitDisputeRequestValidator _validator = new();

        [Test]
        public void Validate_WhenRequestIsValidAtMaximumLengths_ShouldHaveNoErrors()
        {
            SubmitDisputeRequest request = new SubmitDisputeRequestBuilder()
                .WithReason(new string('r', 100))
                .WithExplanation(new string('e', 500))
                .WithIdempotencyKey(new string('k', 200))
                .Build();

            ValidationResult result = _validator.Validate(request);

            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void Validate_WhenExplanationIsNull_ShouldHaveNoErrors()
        {
            ValidationResult result = _validator.Validate(new SubmitDisputeRequestBuilder().WithExplanation(null).Build());

            Assert.That(result.IsValid, Is.True);
        }

        [TestCase("", "reason")]
        [TestCase("   ", "reason")]
        public void Validate_WhenReasonIsBlank_ShouldReturnReasonRequired(string reason, string field)
        {
            AssertSingleError(new SubmitDisputeRequestBuilder().WithReason(reason).Build(), field, "A dispute reason is required.");
        }

        [Test]
        public void Validate_WhenReasonIsTooLong_ShouldReturnReasonTooLong()
        {
            AssertSingleError(new SubmitDisputeRequestBuilder().WithReason(new string('r', 101)).Build(), "reason", "The dispute reason cannot exceed 100 characters.");
        }

        [Test]
        public void Validate_WhenExplanationIsTooLong_ShouldReturnExplanationTooLong()
        {
            AssertSingleError(new SubmitDisputeRequestBuilder().WithExplanation(new string('e', 501)).Build(), "explanation", "The explanation cannot exceed 500 characters.");
        }

        [TestCase("")]
        [TestCase("   ")]
        public void Validate_WhenIdempotencyKeyIsBlank_ShouldReturnIdempotencyKeyRequired(string key)
        {
            AssertSingleError(new SubmitDisputeRequestBuilder().WithIdempotencyKey(key).Build(), "Idempotency-Key", "A valid idempotency key is required.");
        }

        [Test]
        public void Validate_WhenIdempotencyKeyIsTooLong_ShouldReturnIdempotencyKeyRequired()
        {
            AssertSingleError(new SubmitDisputeRequestBuilder().WithIdempotencyKey(new string('k', 201)).Build(), "Idempotency-Key", "A valid idempotency key is required.");
        }

        [Test]
        public void Validate_WhenTransactionIdIsEmpty_ShouldReturnTransactionRequired()
        {
            AssertSingleError(new SubmitDisputeRequestBuilder().WithTransactionId(Guid.Empty).Build(), "transactionId", "A valid transaction is required.");
        }

        [Test]
        public void Validate_WhenCustomerIdIsNotPositive_ShouldReturnCustomerRequired()
        {
            AssertSingleError(new SubmitDisputeRequestBuilder().WithCustomerId(0).Build(), "customerId", "A valid customer is required.");
        }

        private void AssertSingleError(SubmitDisputeRequest request, string field, string message)
        {
            ValidationResult result = _validator.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo(field));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo(message));
        }
    }
}
