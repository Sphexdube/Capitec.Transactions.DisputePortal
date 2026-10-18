using System.Globalization;
using Capitec.Transactions.DisputePortal.Domain.Transactions;

namespace Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Transactions
{
    /// <summary>A customer transaction.</summary>
    public sealed record TransactionSummaryResponse
    {
        /// <summary>The external id of the transaction.</summary>
        public required Guid TransactionId { get; init; }

        /// <summary>The merchant name.</summary>
        public required string Merchant { get; init; }

        /// <summary>The transaction description.</summary>
        public required string Description { get; init; }

        /// <summary>The time the transaction occurred.</summary>
        public required DateTimeOffset OccurredAt { get; init; }

        /// <summary>The amount in minor units as a decimal string, for example 125050.</summary>
        public required string AmountMinor { get; init; }

        /// <summary>The ISO 4217 currency code.</summary>
        public required string CurrencyCode { get; init; }

        /// <summary>The transaction state.</summary>
        public required string State { get; init; }

        public static TransactionSummaryResponse From(TransactionSummary transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            return new TransactionSummaryResponse
            {
                TransactionId = transaction.ExternalId,
                Merchant = transaction.Merchant,
                Description = transaction.Description,
                OccurredAt = transaction.OccurredAt,
                AmountMinor = transaction.AmountMinor.ToString(CultureInfo.InvariantCulture),
                CurrencyCode = transaction.CurrencyCode,
                State = transaction.State
            };
        }
    }
}
