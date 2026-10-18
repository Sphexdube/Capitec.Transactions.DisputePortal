namespace Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions
{
    public sealed class DemoCustomerNotFoundException(string message) : RequestRejectedException(message);
}
