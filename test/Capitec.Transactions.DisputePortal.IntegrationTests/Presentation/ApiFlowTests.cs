using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Capitec.Transactions.DisputePortal.IntegrationTests.Presentation
{
    public sealed class ApiFlowTests
    {
        private const string AlexExample = "Alex Example";
        private const string SamSample = "Sam Sample";

        private static readonly TimeSpan ConfirmationTimeout = TimeSpan.FromSeconds(20);
        private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

        [Test]
        public async Task DemoSession_WhenCustomersAreSeeded_ShouldListThemAndIssueToken()
        {
            using (ApiClient api = CreateClient())
            {
                using (JsonDocument customers = await api.GetJsonAsync("/api/v1/demo-session"))
                {
                    Assert.That(customers.RootElement.GetProperty("data").GetArrayLength(), Is.GreaterThanOrEqualTo(2));
                }

                long customerId = await api.SignInAsDemoCustomerAsync(AlexExample);

                Assert.That(customerId, Is.GreaterThan(0));
                Assert.That(api.Client.DefaultRequestHeaders.Authorization!.Parameter!.Split('.'), Has.Length.EqualTo(3));
            }
        }

        [Test]
        public async Task DemoSession_WhenCustomerIsUnknown_ShouldReturnInvalidDemoCustomer()
        {
            using (ApiClient api = CreateClient())
            {
                using (HttpResponseMessage response = await api.Client.PostAsync(
                    "/api/v1/demo-session",
                    new StringContent("{\"customerId\":999999}", System.Text.Encoding.UTF8, "application/json")))
                {
                    using (JsonDocument document = await ApiClient.ReadJsonAsync(response))
                    {
                        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                        Assert.That(document.RootElement.GetProperty("errors")[0].GetProperty("code").GetString(), Is.EqualTo("INVALID_DEMO_CUSTOMER"));
                    }
                }
            }
        }

        [Test]
        public async Task Transactions_WhenPaging_ShouldReturnNextCursorUntilLastPage()
        {
            using (ApiClient api = CreateClient())
            {
                await api.SignInAsDemoCustomerAsync(SamSample);

                using (JsonDocument firstPage = await api.GetJsonAsync("/api/v1/transaction?pageSize=15"))
                {
                    JsonElement data = firstPage.RootElement.GetProperty("data");
                    string cursor = data.GetProperty("nextCursor").GetString()!;
                    Assert.That(data.GetProperty("items").GetArrayLength(), Is.EqualTo(15));
                    Assert.That(data.GetProperty("items")[0].GetProperty("amountMinor").ValueKind, Is.EqualTo(JsonValueKind.String));

                    using (JsonDocument lastPage = await api.GetJsonAsync($"/api/v1/transaction?pageSize=15&cursor={cursor}"))
                    {
                        JsonElement lastData = lastPage.RootElement.GetProperty("data");
                        Assert.That(lastData.GetProperty("items").GetArrayLength(), Is.EqualTo(8));
                        Assert.That(lastData.GetProperty("nextCursor").ValueKind, Is.EqualTo(JsonValueKind.Null));
                    }
                }
            }
        }

        [Test]
        public async Task SubmitDispute_WhenFlowRunsEndToEnd_ShouldReplayConflictAndRecordConfirmation()
        {
            using (ApiClient api = CreateClient())
            {
                await api.SignInAsDemoCustomerAsync(AlexExample);
                string transactionId = await GetUndisputedTransactionIdAsync(api);
                string key = Guid.NewGuid().ToString("N");
                const string Body = "{\"reason\":\"Unrecognised\",\"explanation\":\"Not mine\"}";

                string disputeId;
                using (HttpResponseMessage accepted = await api.SubmitDisputeAsync(transactionId, key, Body))
                {
                    using (JsonDocument document = await ApiClient.ReadJsonAsync(accepted))
                    {
                        disputeId = document.RootElement.GetProperty("data").GetProperty("disputeId").GetString()!;
                        Assert.That(accepted.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
                        Assert.That(accepted.Headers.Location!.ToString(), Is.EqualTo($"/api/v1/dispute/{disputeId}"));
                    }
                }

                using (HttpResponseMessage replay = await api.SubmitDisputeAsync(transactionId, key, Body))
                {
                    using (JsonDocument document = await ApiClient.ReadJsonAsync(replay))
                    {
                        Assert.That(replay.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
                        Assert.That(document.RootElement.GetProperty("data").GetProperty("disputeId").GetString(), Is.EqualTo(disputeId));
                    }
                }

                await AssertErrorAsync(await api.SubmitDisputeAsync(transactionId, key, "{\"reason\":\"Changed\"}"), HttpStatusCode.Conflict, "IDEMPOTENCY_CONFLICT");
                await AssertErrorAsync(await api.SubmitDisputeAsync(transactionId, Guid.NewGuid().ToString("N"), Body), HttpStatusCode.Conflict, "ACTIVE_DISPUTE_EXISTS");

                using (JsonDocument transaction = await api.GetJsonAsync($"/api/v1/transaction/{transactionId}"))
                {
                    Assert.That(transaction.RootElement.GetProperty("data").GetProperty("transactionId").GetString(), Is.EqualTo(transactionId));
                }

                using (JsonDocument detail = await api.GetJsonAsync($"/api/v1/dispute/{disputeId}"))
                {
                    Assert.That(detail.RootElement.GetProperty("data").GetProperty("status").GetString(), Is.EqualTo("Submitted"));
                    Assert.That(detail.RootElement.GetProperty("data").GetProperty("transaction").GetProperty("transactionId").GetString(), Is.EqualTo(transactionId));
                }

                using (JsonDocument history = await api.GetJsonAsync($"/api/v1/dispute/{disputeId}/history"))
                {
                    Assert.That(history.RootElement.GetProperty("data")[0].GetProperty("actor").GetString(), Is.EqualTo("Customer"));
                }

                using (JsonDocument list = await api.GetJsonAsync("/api/v1/dispute?pageSize=50"))
                {
                    Assert.That(
                        list.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().Select(item => item.GetProperty("disputeId").GetString()),
                        Does.Contain(disputeId));
                }

                Assert.That(await WaitForConfirmationAsync(api, disputeId), Is.EqualTo("Recorded"));
            }
        }

        [Test]
        public async Task SubmitDispute_WhenRequestsRunConcurrently_ShouldReplaySameKeyAndRejectOtherKeys()
        {
            const int ConcurrentRetries = 100;
            const int ConcurrentRequests = 6;
            const string Body = "{\"reason\":\"I was charged more than once\",\"explanation\":null}";

            using (ApiClient api = CreateClient())
            {
                await api.SignInAsDemoCustomerAsync(SamSample);

                string sameKeyTransactionId = await GetUndisputedTransactionIdAsync(api);
                string idempotencyKey = Guid.NewGuid().ToString();
                HttpResponseMessage[] sameKeyResponses = await Task.WhenAll(Enumerable.Range(0, ConcurrentRetries)
                    .Select(_ => api.SubmitDisputeAsync(sameKeyTransactionId, idempotencyKey, Body)));
                HashSet<string> disputeIds = new(StringComparer.Ordinal);

                foreach (HttpResponseMessage response in sameKeyResponses)
                {
                    using (response)
                    {
                        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

                        using (JsonDocument document = await ApiClient.ReadJsonAsync(response))
                        {
                            disputeIds.Add(document.RootElement.GetProperty("data").GetProperty("disputeId").GetString()!);
                        }
                    }
                }

                Assert.That(disputeIds, Has.Count.EqualTo(1));

                string otherKeysTransactionId = await GetUndisputedTransactionIdAsync(api);
                HttpResponseMessage[] otherKeyResponses = await Task.WhenAll(Enumerable.Range(0, ConcurrentRequests)
                    .Select(_ => api.SubmitDisputeAsync(otherKeysTransactionId, Guid.NewGuid().ToString(), Body)));
                HttpStatusCode[] statusCodes = otherKeyResponses.Select(response => response.StatusCode).ToArray();

                foreach (HttpResponseMessage response in otherKeyResponses)
                {
                    response.Dispose();
                }

                Assert.That(statusCodes.Count(statusCode => statusCode == HttpStatusCode.Accepted), Is.EqualTo(1));
                Assert.That(statusCodes.Count(statusCode => statusCode == HttpStatusCode.Conflict), Is.EqualTo(ConcurrentRequests - 1));
            }
        }

        [Test]
        public async Task SubmitDispute_WhenInputIsInvalid_ShouldReturnFieldErrors()
        {
            using (ApiClient api = CreateClient())
            {
                await api.SignInAsDemoCustomerAsync(SamSample);
                string transactionId = await GetUndisputedTransactionIdAsync(api);

                await AssertErrorAsync(await api.SubmitDisputeAsync(transactionId, "k", "{\"reason\":\"  \"}"), HttpStatusCode.BadRequest, "VALIDATION_ERROR", "reason:");
                await AssertErrorAsync(await api.SubmitDisputeAsync(transactionId, "k", "{}"), HttpStatusCode.BadRequest, "VALIDATION_ERROR", "The request is invalid.");
                await AssertErrorAsync(await api.SubmitDisputeAsync(transactionId, null, "{\"reason\":\"x\"}"), HttpStatusCode.BadRequest, "VALIDATION_ERROR", "Idempotency-Key");
                await AssertErrorAsync(await api.SubmitDisputeAsync(Guid.NewGuid().ToString(), "k", "{\"reason\":\"x\"}"), HttpStatusCode.NotFound, "NOT_FOUND");
            }
        }

        [TestCase("/api/v1/transaction?pageSize=51", "pageSize")]
        [TestCase("/api/v1/transaction?cursor=invalid", "cursor")]
        [TestCase("/api/v1/transaction?from=2026-10-10T00:00:00Z&to=2026-10-01T00:00:00Z", "to")]
        [TestCase("/api/v1/dispute?pageSize=0", "pageSize")]
        public async Task Queries_WhenPagingInputIsInvalid_ShouldReturnFieldValidationError(string path, string field)
        {
            using (ApiClient api = CreateClient())
            {
                await api.SignInAsDemoCustomerAsync(AlexExample);

                using (HttpResponseMessage response = await api.Client.GetAsync(path))
                {
                    await AssertErrorAsync(response, HttpStatusCode.BadRequest, "VALIDATION_ERROR", field + ":");
                }
            }
        }

        [Test]
        public async Task Endpoints_WhenDisputeIsUnknown_ShouldReturnNotFound()
        {
            using (ApiClient api = CreateClient())
            {
                await api.SignInAsDemoCustomerAsync(AlexExample);
                Guid unknown = Guid.NewGuid();

                await AssertErrorAsync(await api.Client.GetAsync($"/api/v1/dispute/{unknown}"), HttpStatusCode.NotFound, "NOT_FOUND");
                await AssertErrorAsync(await api.Client.GetAsync($"/api/v1/dispute/{unknown}/history"), HttpStatusCode.NotFound, "NOT_FOUND");
                await AssertErrorAsync(await api.Client.GetAsync($"/api/v1/confirmation/{unknown}"), HttpStatusCode.NotFound, "NOT_FOUND");
                await AssertErrorAsync(await api.Client.GetAsync($"/api/v1/transaction/{unknown}"), HttpStatusCode.NotFound, "NOT_FOUND");
                await AssertErrorAsync(await api.Client.GetAsync("/api/v1/transaction/20000000-0000-0000-0002-000000000001"), HttpStatusCode.NotFound, "NOT_FOUND");
            }
        }

        [TestCase(null)]
        [TestCase("not-a-token")]
        public async Task Endpoints_WhenTokenIsMissingOrInvalid_ShouldReturnUnauthorizedEnvelope(string? token)
        {
            using (ApiClient api = CreateClient())
            {
                if (token is not null)
                {
                    api.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                using (HttpResponseMessage response = await api.Client.GetAsync("/api/v1/dispute"))
                {
                    Assert.That(response.Headers.Contains("CorrelationId"), Is.True);
                    await AssertErrorAsync(response, HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
                }
            }
        }

        [Test]
        public async Task Request_WhenCorrelationIdIsSent_ShouldEchoIt()
        {
            using (ApiClient api = CreateClient())
            {
                api.Client.DefaultRequestHeaders.Add("CorrelationId", "integration-trace");

                using (JsonDocument document = await api.GetJsonAsync("/api/v1/demo-session"))
                {
                    Assert.That(document.RootElement.GetProperty("correlationId").GetString(), Is.EqualTo("integration-trace"));
                }
            }
        }

        [TestCase("/api/v1/health/live")]
        [TestCase("/api/v1/health/ready")]
        public async Task Health_WhenDatabaseIsUp_ShouldReturnHealthyAnonymously(string path)
        {
            using (ApiClient api = CreateClient())
            {
                using (HttpResponseMessage response = await api.Client.GetAsync(path))
                {
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Healthy"));
                }
            }
        }

        [Test]
        public async Task Swagger_WhenRunningInDevelopment_ShouldDocumentEveryEndpoint()
        {
            using (ApiClient api = CreateClient())
            {
                using (JsonDocument document = await api.GetJsonAsync("/swagger/v1/swagger.json"))
                {
                    JsonElement paths = document.RootElement.GetProperty("paths");
                    JsonElement submit = paths.GetProperty("/api/v1/transaction/{transactionId}/dispute").GetProperty("post");

                    Assert.That(paths.EnumerateObject().Count(), Is.EqualTo(8));
                    Assert.That(submit.GetProperty("summary").GetString(), Is.Not.Empty);
                    Assert.That(submit.GetProperty("security").GetArrayLength(), Is.EqualTo(1));
                    Assert.That(paths.GetProperty("/api/v1/demo-session").GetProperty("get").TryGetProperty("security", out _), Is.False);
                    Assert.That(document.RootElement.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _), Is.True);
                }

                using (HttpResponseMessage ui = await api.Client.GetAsync("/swagger/index.html"))
                {
                    Assert.That(ui.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                }
            }
        }

        [Test]
        public async Task Root_WhenRunningInDevelopment_ShouldRedirectToSwagger()
        {
            using (HttpClient client = ApiFactoryFixture.Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false }))
            {
                using (HttpResponseMessage response = await client.GetAsync("/"))
                {
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
                    Assert.That(response.Headers.Location!.ToString(), Is.EqualTo("/swagger"));
                }
            }
        }

        private static ApiClient CreateClient()
        {
            return new ApiClient(ApiFactoryFixture.Factory.CreateClient());
        }

        private static async Task<string> GetUndisputedTransactionIdAsync(ApiClient api)
        {
            HashSet<string> disputed;
            using (JsonDocument disputes = await api.GetJsonAsync("/api/v1/dispute?pageSize=50"))
            {
                disputed = disputes.RootElement.GetProperty("data").GetProperty("items").EnumerateArray()
                    .Select(item => item.GetProperty("transaction").GetProperty("transactionId").GetString()!)
                    .ToHashSet(StringComparer.Ordinal);
            }

            using (JsonDocument transactions = await api.GetJsonAsync("/api/v1/transaction?pageSize=50"))
            {
                return transactions.RootElement.GetProperty("data").GetProperty("items").EnumerateArray()
                    .Select(item => item.GetProperty("transactionId").GetString()!)
                    .First(id => !disputed.Contains(id));
            }
        }

        private static async Task<string> WaitForConfirmationAsync(ApiClient api, string disputeId)
        {
            DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(ConfirmationTimeout);
            string status = string.Empty;

            while (DateTimeOffset.UtcNow < deadline)
            {
                using (JsonDocument confirmation = await api.GetJsonAsync($"/api/v1/confirmation/{disputeId}"))
                {
                    status = confirmation.RootElement.GetProperty("data").GetProperty("status").GetString()!;
                }

                if (status == "Recorded")
                {
                    return status;
                }

                await Task.Delay(PollInterval);
            }

            return status;
        }

        private static async Task AssertErrorAsync(HttpResponseMessage response, HttpStatusCode statusCode, string code, string? messageFragment = null)
        {
            using (response)
            {
                using (JsonDocument document = await ApiClient.ReadJsonAsync(response))
                {
                    JsonElement error = document.RootElement.GetProperty("errors")[0];

                    Assert.That(response.StatusCode, Is.EqualTo(statusCode));
                    Assert.That(error.GetProperty("code").GetString(), Is.EqualTo(code));
                    Assert.That(document.RootElement.GetProperty("correlationId").GetString(), Is.Not.Empty);

                    if (messageFragment is not null)
                    {
                        Assert.That(error.GetProperty("message").GetString(), Does.Contain(messageFragment));
                    }
                }
            }
        }
    }
}
