namespace Capitec.Transactions.DisputePortal.Domain.Exceptions
{
    public sealed class UniqueConstraintException(string message, Exception innerException)
        : Exception(message, innerException);
}
