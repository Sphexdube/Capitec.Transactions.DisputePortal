using Capitec.Transactions.DisputePortal.Api.Results;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;

namespace Capitec.Transactions.DisputePortal.Api.Middleware
{
    public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            try
            {
                await next(context);
            }
            catch (Exception exception) when (!context.Response.HasStarted)
            {
                ApiExceptionMapping mapping = ApiExceptionMapper.Map(exception);

                if (mapping.StatusCode >= StatusCodes.Status500InternalServerError)
                {
                    logger.LogError(exception, ErrorMessages.ApiRequestFailed, context.TraceIdentifier);
                }
                else
                {
                    logger.LogInformation(
                        InfoMessages.ApiRequestRejected,
                        mapping.StatusCode,
                        string.Join(',', mapping.Errors.Select(error => error.Code).Distinct()),
                        context.TraceIdentifier);
                }

                context.Response.Clear();
                context.Response.StatusCode = mapping.StatusCode;
                await context.Response.WriteAsJsonAsync(ApiResponseFactory.CreateErrors(context, mapping.Errors));
            }
        }
    }
}
