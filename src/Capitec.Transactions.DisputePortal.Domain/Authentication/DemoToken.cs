namespace Capitec.Transactions.DisputePortal.Domain.Authentication
{
    public sealed record DemoToken(string AccessToken, DateTimeOffset ExpiresAt);
}
