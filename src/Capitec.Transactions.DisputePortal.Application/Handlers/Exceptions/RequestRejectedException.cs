namespace Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions
{
    /// <summary>An expected outcome the API reports to the caller as a 4xx response, not a failure.</summary>
    public abstract class RequestRejectedException(string message) : Exception(message);
}
