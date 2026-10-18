using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;

namespace Capitec.Transactions.DisputePortal.Domain.Transactions
{
    public sealed record Currency
    {
        private const int CodeLength = 3;
        private const int MaximumMinorUnitExponent = 4;

        public string Code { get; }

        public int MinorUnitExponent { get; }

        public Currency(string code, int minorUnitExponent)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != CodeLength || code.Any(character => !char.IsAsciiLetter(character)))
            {
                throw new ArgumentException(ValidationMessages.CurrencyCodeInvalid, nameof(code));
            }

            if (minorUnitExponent < 0 || minorUnitExponent > MaximumMinorUnitExponent)
            {
                throw new ArgumentOutOfRangeException(nameof(minorUnitExponent));
            }

            Code = code.ToUpperInvariant();
            MinorUnitExponent = minorUnitExponent;
        }
    }
}
