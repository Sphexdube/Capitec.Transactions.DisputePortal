namespace Capitec.Transactions.DisputePortal.Application.Handlers
{
    public interface IRequestHandler<TRequest, TResponse>
    {
        public Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
