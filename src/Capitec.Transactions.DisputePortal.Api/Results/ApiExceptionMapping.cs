using Capitec.Transactions.DisputePortal.Application.Models.Response.V1;

namespace Capitec.Transactions.DisputePortal.Api.Results
{
    public sealed record ApiExceptionMapping(int StatusCode, IReadOnlyList<ApiError> Errors);
}
