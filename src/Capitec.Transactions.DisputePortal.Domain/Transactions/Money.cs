using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;

namespace Capitec.Transactions.DisputePortal.Domain.Transactions
{
    public sealed record Money
    {
        public long AmountMinor { get; }

        public Currency Currency { get; }

        public Money(long amountMinor, Currency currency)
        {
            ArgumentNullException.ThrowIfNull(currency);
            AmountMinor = amountMinor;
            Currency = currency;
        }

        public Money Add(Money other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (Currency != other.Currency)
            {
                throw new InvalidOperationException(ErrorMessages.MoneyCurrencyMismatch);
            }

            return new Money(checked(AmountMinor + other.AmountMinor), Currency);
        }
    }
}
