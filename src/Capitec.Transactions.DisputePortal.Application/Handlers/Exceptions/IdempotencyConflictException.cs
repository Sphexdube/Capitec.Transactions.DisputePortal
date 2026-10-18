namespace Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions
{
    public sealed class IdempotencyConflictException(string message) : RequestRejectedException(message);
}
