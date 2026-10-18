namespace Capitec.Transactions.DisputePortal.Api.Middleware
{
    public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        public const string HeaderName = "CorrelationId";
        public const int MaximumLength = 128;

        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            string? requestedId = context.Request.Headers[HeaderName].FirstOrDefault();
            string correlationId = !string.IsNullOrWhiteSpace(requestedId) && requestedId.Length <= MaximumLength
                ? requestedId
                : Guid.NewGuid().ToString("N");

            context.TraceIdentifier = correlationId;
            context.Response.Headers[HeaderName] = correlationId;

            using (logger.BeginScope(new Dictionary<string, object> { [HeaderName] = correlationId }))
            {
                await next(context);
            }
        }
    }
}
