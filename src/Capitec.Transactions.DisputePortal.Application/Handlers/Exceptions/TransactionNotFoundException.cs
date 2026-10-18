namespace Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions
{
    public sealed class TransactionNotFoundException(string message) : RequestRejectedException(message);
}
