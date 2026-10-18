using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1;
using Capitec.Transactions.DisputePortal.Domain.Errors;
using Capitec.Transactions.DisputePortal.Domain.Exceptions;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using FluentValidation;

namespace Capitec.Transactions.DisputePortal.Api.Results
{
    public static class ApiExceptionMapper
    {
        public static ApiExceptionMapping Map(Exception exception)
        {
            return exception switch
            {
                ValidationException validationException => new ApiExceptionMapping(
                    StatusCodes.Status400BadRequest,
                    ApiResponseFactory.CreateValidationErrors(validationException.Errors)),
                TransactionNotFoundException => Single(
                    StatusCodes.Status404NotFound,
                    ApiErrorCodes.NotFound,
                    ErrorMessages.TransactionNotFoundDetail),
                DisputeNotFoundException => Single(
                    StatusCodes.Status404NotFound,
                    ApiErrorCodes.NotFound,
                    ErrorMessages.DisputeNotFoundDetail),
                ActiveDisputeExistsException => Single(
                    StatusCodes.Status409Conflict,
                    ApiErrorCodes.ActiveDisputeExists,
                    ErrorMessages.ActiveDisputeExists),
                IdempotencyConflictException => Single(
                    StatusCodes.Status409Conflict,
                    ApiErrorCodes.IdempotencyConflict,
                    ErrorMessages.IdempotencyConflictDetail),
                DemoCustomerNotFoundException => Single(
                    StatusCodes.Status400BadRequest,
                    ApiErrorCodes.InvalidDemoCustomer,
                    ErrorMessages.InvalidDemoCustomerDetail),
                UniqueConstraintException => Single(
                    StatusCodes.Status409Conflict,
                    ApiErrorCodes.ConcurrentRequest,
                    ErrorMessages.ConcurrentRequestDetail),
                SaveContextException => Single(
                    StatusCodes.Status503ServiceUnavailable,
                    ApiErrorCodes.PersistenceUnavailable,
                    ErrorMessages.PersistenceUnavailableDetail),
                _ => Single(
                    StatusCodes.Status500InternalServerError,
                    ApiErrorCodes.InternalError,
                    ErrorMessages.InternalErrorDetail)
            };
        }

        private static ApiExceptionMapping Single(int statusCode, string code, string message)
        {
            return new ApiExceptionMapping(statusCode, [ApiResponseFactory.CreateApiError(code, message)]);
        }
    }
}
