using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Paging;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using FluentValidation;

namespace Capitec.Transactions.DisputePortal.Application.Validators.V1
{
    public sealed class GetTransactionsRequestValidator : AbstractValidator<GetTransactionsRequest>
    {
        public GetTransactionsRequestValidator()
        {
            RuleFor(request => request.CustomerId)
                .GreaterThan(0)
                .WithMessage(ValidationMessages.CustomerRequired)
                .OverridePropertyName(FieldNames.CustomerId);

            RuleFor(request => request.PageSize)
                .InclusiveBetween(PageLimits.MinimumPageSize, PageLimits.MaximumPageSize)
                .WithMessage(ValidationMessages.PageSizeOutOfRange)
                .OverridePropertyName(FieldNames.PageSize);

            RuleFor(request => request.Cursor)
                .Must(PageCursorCodec.IsValidOrEmpty)
                .WithMessage(ValidationMessages.TransactionCursorInvalid)
                .OverridePropertyName(FieldNames.Cursor);

            RuleFor(request => request.To)
                .Must((request, to) => !request.From.HasValue || !to.HasValue || request.From.Value <= to.Value)
                .WithMessage(ValidationMessages.EndDateBeforeStartDate)
                .OverridePropertyName(FieldNames.To);
        }
    }
}
