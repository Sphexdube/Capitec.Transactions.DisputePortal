namespace Capitec.Transactions.DisputePortal.Application.Models.Response.V1
{
    /// <summary>The envelope for every successful request.</summary>
    /// <typeparam name="TData">The type of the response data.</typeparam>
    public sealed record ApiSuccessResponse<TData>
    {
        /// <summary>The response data.</summary>
        public required TData Data { get; init; }

        /// <summary>The correlation id of the request, also returned in the CorrelationId header.</summary>
        public required string CorrelationId { get; init; }
    }
}
