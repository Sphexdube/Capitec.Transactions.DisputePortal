using Capitec.Transactions.DisputePortal.Api.Results;
using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Capitec.Transactions.DisputePortal.UnitTests.Presentation.Results
{
    public sealed class ApiExceptionMapperTests
    {
        private static IEnumerable<TestCaseData> MappedExceptions()
        {
            yield return new TestCaseData(new TransactionNotFoundException("x"), StatusCodes.Status404NotFound, "NOT_FOUND");
            yield return new TestCaseData(new DisputeNotFoundException("x"), StatusCodes.Status404NotFound, "NOT_FOUND");
            yield return new TestCaseData(new ActiveDisputeExistsException("x"), StatusCodes.Status409Conflict, "ACTIVE_DISPUTE_EXISTS");
            yield return new TestCaseData(new IdempotencyConflictException("x"), StatusCodes.Status409Conflict, "IDEMPOTENCY_CONFLICT");
            yield return new TestCaseData(new DemoCustomerNotFoundException("x"), StatusCodes.Status400BadRequest, "INVALID_DEMO_CUSTOMER");
            yield return new TestCaseData(new UniqueConstraintException("x", new TimeoutException()), StatusCodes.Status409Conflict, "CONCURRENT_REQUEST");
            yield return new TestCaseData(new SaveContextException("x", new TimeoutException()), StatusCodes.Status503ServiceUnavailable, "PERSISTENCE_UNAVAILABLE");
            yield return new TestCaseData(new InvalidOperationException("secret detail"), StatusCodes.Status500InternalServerError, "INTERNAL_ERROR");
        }

        [TestCaseSource(nameof(MappedExceptions))]
        public void Map_WhenExceptionIsKnown_ShouldReturnStableStatusAndCode(Exception exception, int statusCode, string code)
        {
            ApiExceptionMapping mapping = ApiExceptionMapper.Map(exception);

            Assert.That(mapping.StatusCode, Is.EqualTo(statusCode));
            Assert.That(mapping.Errors.Single().Code, Is.EqualTo(code));
            Assert.That(mapping.Errors.Single().Message, Does.Not.Contain("secret"));
        }

        [Test]
        public void Map_WhenValidationFails_ShouldReturnOneErrorPerFailure()
        {
            ValidationException exception = new([new ValidationFailure("reason", "Required."), new ValidationFailure(string.Empty, "Whole request.")]);

            ApiExceptionMapping mapping = ApiExceptionMapper.Map(exception);

            Assert.That(mapping.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(mapping.Errors.Select(error => error.Message), Is.EqualTo(new[] { "reason: Required.", "Whole request." }));
        }
    }
}
