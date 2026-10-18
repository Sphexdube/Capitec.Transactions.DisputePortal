using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using FluentValidation;

namespace Capitec.Transactions.DisputePortal.Application.Validators.V1
{
    public sealed class SubmitDisputeRequestValidator : AbstractValidator<SubmitDisputeRequest>
    {
        public SubmitDisputeRequestValidator()
        {
            RuleFor(request => request.TransactionId)
                .NotEqual(Guid.Empty)
                .WithMessage(ValidationMessages.TransactionRequired)
                .OverridePropertyName(FieldNames.TransactionId);

            RuleFor(request => request.CustomerId)
                .GreaterThan(0)
                .WithMessage(ValidationMessages.CustomerRequired)
                .OverridePropertyName(FieldNames.CustomerId);

            RuleFor(request => request.IdempotencyKey)
                .Must(key => !string.IsNullOrWhiteSpace(key) && key.Length <= DisputeLimits.IdempotencyKeyMaximumLength)
                .WithMessage(ValidationMessages.IdempotencyKeyRequired)
                .OverridePropertyName(FieldNames.IdempotencyKey);

            RuleFor(request => request.Reason)
                .Cascade(CascadeMode.Stop)
                .Must(reason => !string.IsNullOrWhiteSpace(reason))
                .WithMessage(ValidationMessages.DisputeReasonRequired)
                .Must(reason => reason.Trim().Length <= DisputeLimits.ReasonMaximumLength)
                .WithMessage(ValidationMessages.DisputeReasonTooLong)
                .OverridePropertyName(FieldNames.Reason);

            RuleFor(request => request.Explanation)
                .Must(explanation => explanation is null || explanation.Trim().Length <= DisputeLimits.ExplanationMaximumLength)
                .WithMessage(ValidationMessages.ExplanationTooLong)
                .OverridePropertyName(FieldNames.Explanation);
        }
    }
}
