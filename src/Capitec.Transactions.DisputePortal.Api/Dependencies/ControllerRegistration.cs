using System.Globalization;
using System.Text.Json.Serialization;
using Capitec.Transactions.DisputePortal.Api.Results;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1;
using Capitec.Transactions.DisputePortal.Domain.Errors;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Capitec.Transactions.DisputePortal.Api.Dependencies
{
    public static class ControllerRegistration
    {
        private const char JsonPathRoot = '$';
        private const char JsonPathSeparator = '.';

        public static void Register(IServiceCollection services)
        {
            services
                .AddControllers()
                .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = CreateInvalidModelStateResult)
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        }

        private static IActionResult CreateInvalidModelStateResult(ActionContext context)
        {
            HashSet<string> bodyParameterNames = context.ActionDescriptor.Parameters
                .Where(parameter => parameter.BindingInfo?.BindingSource == BindingSource.Body)
                .Select(parameter => parameter.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            ApiError[] errors = context.ModelState.Keys
                .Where(key => context.ModelState.GetFieldValidationState(key) == ModelValidationState.Invalid)
                .Select(ToFieldName)
                .Where(fieldName => fieldName.Length > 0 && !bodyParameterNames.Contains(fieldName))
                .Distinct(StringComparer.Ordinal)
                .Select(fieldName => ApiResponseFactory.CreateApiError(
                    ApiErrorCodes.ValidationError,
                    string.Format(CultureInfo.InvariantCulture, ErrorMessages.FieldInvalidFormat, fieldName)))
                .ToArray();

            if (errors.Length == 0)
            {
                errors = [ApiResponseFactory.CreateApiError(ApiErrorCodes.ValidationError, ErrorMessages.RequestInvalid)];
            }

            return new BadRequestObjectResult(ApiResponseFactory.CreateErrors(context.HttpContext, errors));
        }

        private static string ToFieldName(string modelStateKey)
        {
            return modelStateKey.TrimStart(JsonPathRoot, JsonPathSeparator);
        }
    }
}
