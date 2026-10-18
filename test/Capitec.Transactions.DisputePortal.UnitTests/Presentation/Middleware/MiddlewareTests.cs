using System.Text.Json;
using Capitec.Transactions.DisputePortal.Api.Middleware;
using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;

namespace Capitec.Transactions.DisputePortal.UnitTests.Presentation.Middleware
{
    public sealed class MiddlewareTests
    {
        [Test]
        public async Task CorrelationId_WhenHeaderIsValid_ShouldReuseIt()
        {
            DefaultHttpContext context = new();
            context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "abc-123";
            CorrelationIdMiddleware middleware = new(_ => Task.CompletedTask, NullLogger<CorrelationIdMiddleware>.Instance);

            await middleware.InvokeAsync(context);

            Assert.That(context.TraceIdentifier, Is.EqualTo("abc-123"));
            Assert.That(context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString(), Is.EqualTo("abc-123"));
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task CorrelationId_WhenHeaderIsMissing_ShouldGenerateOne(string? header)
        {
            DefaultHttpContext context = new();
            context.Request.Headers[CorrelationIdMiddleware.HeaderName] = header;
            CorrelationIdMiddleware middleware = new(_ => Task.CompletedTask, NullLogger<CorrelationIdMiddleware>.Instance);

            await middleware.InvokeAsync(context);

            Assert.That(context.TraceIdentifier, Has.Length.EqualTo(32));
        }

        [Test]
        public async Task CorrelationId_WhenHeaderIsTooLong_ShouldGenerateOne()
        {
            DefaultHttpContext context = new();
            context.Request.Headers[CorrelationIdMiddleware.HeaderName] = new string('a', 129);
            CorrelationIdMiddleware middleware = new(_ => Task.CompletedTask, NullLogger<CorrelationIdMiddleware>.Instance);

            await middleware.InvokeAsync(context);

            Assert.That(context.TraceIdentifier, Has.Length.EqualTo(32));
        }

        [Test]
        public async Task ExceptionHandling_WhenNextThrows_ShouldWriteErrorEnvelope()
        {
            DefaultHttpContext context = new() { TraceIdentifier = "trace-1" };
            context.Response.Body = new MemoryStream();
            ExceptionHandlingMiddleware middleware = new(_ => throw new InvalidOperationException("boom"), NullLogger<ExceptionHandlingMiddleware>.Instance);

            await middleware.InvokeAsync(context);

            context.Response.Body.Position = 0;
            using (JsonDocument document = await JsonDocument.ParseAsync(context.Response.Body))
            {
                Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
                Assert.That(document.RootElement.GetProperty("errors")[0].GetProperty("code").GetString(), Is.EqualTo("INTERNAL_ERROR"));
                Assert.That(document.RootElement.GetProperty("correlationId").GetString(), Is.EqualTo("trace-1"));
            }
        }

        [Test]
        public async Task ExceptionHandling_WhenRequestIsRejected_ShouldWriteClientErrorEnvelope()
        {
            DefaultHttpContext context = new() { TraceIdentifier = "trace-2" };
            context.Response.Body = new MemoryStream();
            ExceptionHandlingMiddleware middleware = new(_ => throw new DisputeNotFoundException("missing"), NullLogger<ExceptionHandlingMiddleware>.Instance);

            await middleware.InvokeAsync(context);

            context.Response.Body.Position = 0;
            using (JsonDocument document = await JsonDocument.ParseAsync(context.Response.Body))
            {
                Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
                Assert.That(document.RootElement.GetProperty("errors")[0].GetProperty("code").GetString(), Is.EqualTo("NOT_FOUND"));
            }
        }

        [Test]
        public void ExceptionHandling_WhenResponseHasStarted_ShouldRethrow()
        {
            DefaultHttpContext context = new();
            context.Features.Set<IHttpResponseFeature>(new StartedResponseFeature());
            ExceptionHandlingMiddleware middleware = new(_ => throw new InvalidOperationException("boom"), NullLogger<ExceptionHandlingMiddleware>.Instance);

            Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));
        }

        [Test]
        public async Task ExceptionHandling_WhenNextSucceeds_ShouldLeaveResponseUnchanged()
        {
            DefaultHttpContext context = new();
            ExceptionHandlingMiddleware middleware = new(_ => Task.CompletedTask, NullLogger<ExceptionHandlingMiddleware>.Instance);

            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public void Middleware_WhenContextIsNull_ShouldThrowArgumentNullException()
        {
            CorrelationIdMiddleware correlation = new(_ => Task.CompletedTask, NullLogger<CorrelationIdMiddleware>.Instance);
            ExceptionHandlingMiddleware exceptions = new(_ => Task.CompletedTask, NullLogger<ExceptionHandlingMiddleware>.Instance);

            Assert.ThrowsAsync<ArgumentNullException>(() => correlation.InvokeAsync(null!));
            Assert.ThrowsAsync<ArgumentNullException>(() => exceptions.InvokeAsync(null!));
        }

        private sealed class StartedResponseFeature : HttpResponseFeature
        {
            public override bool HasStarted
            {
                get
                {
                    return true;
                }
            }
        }
    }
}
