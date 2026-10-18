namespace Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions
{
    public sealed class DisputeNotFoundException(string message) : RequestRejectedException(message);
}
