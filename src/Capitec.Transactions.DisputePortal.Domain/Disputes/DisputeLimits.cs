namespace Capitec.Transactions.DisputePortal.Domain.Disputes
{
    public static class DisputeLimits
    {
        public const int ReasonMaximumLength = 100;
        public const int ExplanationMaximumLength = 500;
        public const int IdempotencyKeyMaximumLength = 200;
    }
}
