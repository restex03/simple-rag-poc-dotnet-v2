using System.Net.Http;
using System.Text.Json;

public sealed class OpenAiHttpLoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"HTTP {request.Method} {request.RequestUri}");

        if (request.Content is not null)
        {
            var body = await request.Content.ReadAsStringAsync(cancellationToken);

            Console.WriteLine("=== OUTGOING REQUEST ===");

            try
            {
                using var json = JsonDocument.Parse(body);

                Console.WriteLine(JsonSerializer.Serialize(json.RootElement, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
            }
            catch (JsonException)
            {
                Console.WriteLine($"Mission failed: JsonException: {body}");
            }

        }

        var response = await base.SendAsync(request, cancellationToken);

        Console.WriteLine($"Http Response: {(int)response.StatusCode}");

        if (response.Content is not null)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            Console.WriteLine("=== INCOMING RESPONSE ===");
            Console.WriteLine(responseBody);
        }
        return response;
    }
}