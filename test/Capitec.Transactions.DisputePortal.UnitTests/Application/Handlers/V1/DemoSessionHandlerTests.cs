using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Application.Handlers.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.DemoSessions;
using Capitec.Transactions.DisputePortal.Domain.Authentication;
using Capitec.Transactions.DisputePortal.Domain.Customers;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.UnitTests.Setup;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Handlers.V1
{
    public sealed class DemoSessionHandlerTests
    {
        private UnitOfWorkSubstitute _unitOfWork = null!;
        private IDemoTokenIssuer _tokenIssuer = null!;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = new UnitOfWorkSubstitute();
            _tokenIssuer = Substitute.For<IDemoTokenIssuer>();
        }

        [Test]
        public async Task GetDemoCustomers_WhenCustomersExist_ShouldReturnOptions()
        {
            _unitOfWork.Customers.GetAllAsync(Arg.Any<CancellationToken>()).Returns([new CustomerSummary(1, "Alex Example")]);
            GetDemoCustomersHandler handler = new(NullLogger<GetDemoCustomersHandler>.Instance, _unitOfWork.Factory);

            IReadOnlyList<DemoCustomerOptionResponse> customers = await handler.HandleAsync(new GetDemoCustomersRequest());

            Assert.That(customers, Has.Count.EqualTo(1));
            Assert.That(customers[0].CustomerId, Is.EqualTo(1));
            Assert.That(customers[0].DisplayName, Is.EqualTo("Alex Example"));
        }

        [Test]
        public void GetDemoCustomers_WhenRepositoryFails_ShouldRethrowTheFailure()
        {
            _unitOfWork.Customers.GetAllAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new TimeoutException());
            GetDemoCustomersHandler handler = new(NullLogger<GetDemoCustomersHandler>.Instance, _unitOfWork.Factory);

            Assert.ThrowsAsync<TimeoutException>(() => handler.HandleAsync(new GetDemoCustomersRequest()));
        }

        [Test]
        public async Task CreateDemoSession_WhenCustomerExists_ShouldReturnSignedToken()
        {
            DateTimeOffset expiresAt = new(2026, 10, 18, 12, 15, 0, TimeSpan.Zero);
            _unitOfWork.Customers.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(new CustomerSummary(2, "Sam Sample"));
            _tokenIssuer.Issue(2).Returns(new DemoToken("token", expiresAt));
            CreateDemoSessionHandler handler = new(NullLogger<CreateDemoSessionHandler>.Instance, _unitOfWork.Factory, _tokenIssuer);

            DemoSessionResponse session = await handler.HandleAsync(new CreateDemoSessionRequest { CustomerId = 2 });

            Assert.That(session.AccessToken, Is.EqualTo("token"));
            Assert.That(session.DisplayName, Is.EqualTo("Sam Sample"));
            Assert.That(session.ExpiresAt, Is.EqualTo(expiresAt));
        }

        [Test]
        public void CreateDemoSession_WhenCustomerIsUnknown_ShouldThrowDemoCustomerNotFoundException()
        {
            CreateDemoSessionHandler handler = new(NullLogger<CreateDemoSessionHandler>.Instance, _unitOfWork.Factory, _tokenIssuer);

            Assert.ThrowsAsync<DemoCustomerNotFoundException>(() => handler.HandleAsync(new CreateDemoSessionRequest { CustomerId = 999 }));

            _tokenIssuer.DidNotReceive().Issue(Arg.Any<long>());
        }

        [Test]
        public void CreateDemoSession_WhenUnitOfWorkFails_ShouldRethrowTheFailure()
        {
            _unitOfWork.Factory.Create().Throws(new TimeoutException());
            CreateDemoSessionHandler handler = new(NullLogger<CreateDemoSessionHandler>.Instance, _unitOfWork.Factory, _tokenIssuer);

            Assert.ThrowsAsync<TimeoutException>(() => handler.HandleAsync(new CreateDemoSessionRequest { CustomerId = 2 }));
        }

        [Test]
        public void CreateDemoSession_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            CreateDemoSessionHandler handler = new(NullLogger<CreateDemoSessionHandler>.Instance, _unitOfWork.Factory, _tokenIssuer);

            Assert.ThrowsAsync<ArgumentNullException>(() => handler.HandleAsync(null!));
        }
    }
}
