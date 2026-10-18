namespace Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions
{
    public sealed class ActiveDisputeExistsException(string message) : RequestRejectedException(message);
}
