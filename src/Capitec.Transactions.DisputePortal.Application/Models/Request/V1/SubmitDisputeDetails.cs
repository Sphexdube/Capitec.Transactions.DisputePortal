namespace Capitec.Transactions.DisputePortal.Application.Models.Request.V1
{
    /// <summary>The dispute details the customer enters.</summary>
    public sealed record SubmitDisputeDetails
    {
        /// <summary>The reason the customer disputes the transaction, up to 100 characters.</summary>
        public required string Reason { get; init; }

        /// <summary>The optional explanation from the customer, up to 500 characters.</summary>
        public string? Explanation { get; init; }
    }
}
