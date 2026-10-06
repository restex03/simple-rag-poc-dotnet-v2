namespace Rag.Infra.Gateways;

public sealed class OllamaGateway : IOllamaGateway
{

    private readonly IHttpClientFactory _httpClientFactory;
    public OllamaGateway(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public Task<string> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();
        // Implement the actual HTTP request to generate the embedding here
        throw new NotImplementedException();
    }

    public Task<string> CompleteAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();
        // Implement the actual HTTP request to complete the prompt here
        throw new NotImplementedException();
    }
}