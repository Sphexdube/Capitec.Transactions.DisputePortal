using System.Globalization;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1;
using Capitec.Transactions.DisputePortal.Domain.Errors;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using FluentValidation.Results;

namespace Capitec.Transactions.DisputePortal.Api.Results
{
    public static class ApiResponseFactory
    {
        public static ApiSuccessResponse<TData> CreateSuccess<TData>(HttpContext context, TData data)
        {
            ArgumentNullException.ThrowIfNull(context);

            return new ApiSuccessResponse<TData>
            {
                Data = data,
                CorrelationId = context.TraceIdentifier
            };
        }

        public static ApiErrorResponse CreateError(HttpContext context, string code, string message)
        {
            return CreateErrors(context, [CreateApiError(code, message)]);
        }

        public static ApiErrorResponse CreateErrors(HttpContext context, IReadOnlyList<ApiError> errors)
        {
            ArgumentNullException.ThrowIfNull(context);

            return new ApiErrorResponse
            {
                Errors = errors,
                CorrelationId = context.TraceIdentifier
            };
        }

        public static IReadOnlyList<ApiError> CreateValidationErrors(IEnumerable<ValidationFailure> failures)
        {
            return failures
                .Select(failure => CreateApiError(
                    ApiErrorCodes.ValidationError,
                    string.IsNullOrWhiteSpace(failure.PropertyName)
                        ? failure.ErrorMessage
                        : string.Format(CultureInfo.InvariantCulture, ErrorMessages.FieldErrorFormat, failure.PropertyName, failure.ErrorMessage)))
                .ToArray();
        }

        public static ApiError CreateApiError(string code, string message)
        {
            return new ApiError
            {
                Code = code,
                Message = message
            };
        }
    }
}
