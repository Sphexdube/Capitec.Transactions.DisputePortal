using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Capitec.Transactions.DisputePortal.IntegrationTests.Presentation
{
    public sealed class ApiClient(HttpClient client) : IDisposable
    {
        private const string JsonMediaType = "application/json";

        public HttpClient Client { get; } = client;

        public async Task<long> SignInAsDemoCustomerAsync(string displayName)
        {
            using (JsonDocument customers = await GetJsonAsync("/api/v1/demo-session"))
            {
                long customerId = customers.RootElement.GetProperty("data").EnumerateArray()
                    .Single(customer => customer.GetProperty("displayName").GetString() == displayName)
                    .GetProperty("customerId")
                    .GetInt64();

                using (HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/demo-session", new { customerId }))
                {
                    response.EnsureSuccessStatusCode();

                    using (JsonDocument session = await ReadJsonAsync(response))
                    {
                        string token = session.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
                        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                }

                return customerId;
            }
        }

        public async Task<JsonDocument> GetJsonAsync(string path)
        {
            using (HttpResponseMessage response = await Client.GetAsync(path))
            {
                return await ReadJsonAsync(response);
            }
        }

        public async Task<HttpResponseMessage> SubmitDisputeAsync(string transactionId, string? idempotencyKey, string body)
        {
            using (HttpRequestMessage request = new(HttpMethod.Post, $"/api/v1/transaction/{transactionId}/dispute"))
            {
                if (idempotencyKey is not null)
                {
                    request.Headers.Add("Idempotency-Key", idempotencyKey);
                }

                request.Content = new StringContent(body, Encoding.UTF8, JsonMediaType);

                return await Client.SendAsync(request);
            }
        }

        public static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
        {
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}
