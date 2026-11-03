using System.Net;
using System.Text.Json;

namespace Identity.IntegrationTests.Infrastructure
{
    /// <summary> The parts of a ProblemDetails response that must not reveal the cause (traceId is per request and ignored). </summary>
    public sealed record ProblemResponse(HttpStatusCode Status, string? Title, string? Detail)
    {
        public static async Task<ProblemResponse> ReadAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                return new ProblemResponse(response.StatusCode, null, null);

            using var json = JsonDocument.Parse(body);
            return new ProblemResponse(
                response.StatusCode,
                json.RootElement.TryGetProperty("title", out var title) ? title.GetString() : null,
                json.RootElement.TryGetProperty("detail", out var detail) ? detail.GetString() : null);
        }
    }
}
