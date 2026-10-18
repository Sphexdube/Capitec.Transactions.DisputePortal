namespace Capitec.Transactions.DisputePortal.Domain.Exceptions
{
    public sealed class SaveContextException(string message, Exception innerException)
        : Exception(message, innerException);
}
