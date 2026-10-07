using System.Net.Http.Json;
namespace Rag.Infra.Gateways;

public sealed class OllamaGateway : IOllamaGateway
{
    private readonly HttpClient _httpClient;

    public OllamaGateway(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = "qwen3-embedding:0.6b",
            input = text
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/api/embed",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>(
                cancellationToken);

        if (result?.Embeddings is null || result.Embeddings.Length == 0)
            throw new InvalidOperationException("Ollama returned no embeddings.");

        return result.Embeddings[0];
    }

    public async Task<string> CompleteAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = "qwen3:4b-instruct-8k",
            prompt,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/api/generate",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(
                cancellationToken);

        return result?.Response
            ?? throw new InvalidOperationException("Ollama returned no completion.");
    }

    private sealed record OllamaEmbedResponse(
        float[][] Embeddings);

    private sealed record OllamaGenerateResponse(
        string Response);
}