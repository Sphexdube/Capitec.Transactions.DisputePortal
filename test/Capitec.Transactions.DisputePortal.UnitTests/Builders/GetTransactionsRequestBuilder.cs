using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;

namespace Capitec.Transactions.DisputePortal.UnitTests.Builders
{
    public sealed class GetTransactionsRequestBuilder
    {
        private long _customerId = 1;
        private string? _cursor;
        private int _pageSize = 2;
        private DateTimeOffset? _from;
        private DateTimeOffset? _to;

        public GetTransactionsRequestBuilder WithCustomerId(long customerId)
        {
            _customerId = customerId;
            return this;
        }

        public GetTransactionsRequestBuilder WithCursor(string? cursor)
        {
            _cursor = cursor;
            return this;
        }

        public GetTransactionsRequestBuilder WithPageSize(int pageSize)
        {
            _pageSize = pageSize;
            return this;
        }

        public GetTransactionsRequestBuilder WithRange(DateTimeOffset? from, DateTimeOffset? to)
        {
            _from = from;
            _to = to;
            return this;
        }

        public GetTransactionsRequest Build()
        {
            return new GetTransactionsRequest
            {
                CustomerId = _customerId,
                Cursor = _cursor,
                PageSize = _pageSize,
                From = _from,
                To = _to
            };
        }
    }
}
