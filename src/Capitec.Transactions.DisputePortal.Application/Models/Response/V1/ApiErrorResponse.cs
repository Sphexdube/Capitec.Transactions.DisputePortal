namespace Capitec.Transactions.DisputePortal.Application.Models.Response.V1
{
    /// <summary>The envelope for every failed request.</summary>
    public sealed record ApiErrorResponse
    {
        /// <summary>The errors that caused the failure.</summary>
        public required IReadOnlyList<ApiError> Errors { get; init; }

        /// <summary>The correlation id of the request, also returned in the CorrelationId header.</summary>
        public required string CorrelationId { get; init; }
    }
}
