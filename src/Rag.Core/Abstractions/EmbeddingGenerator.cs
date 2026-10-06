using System.Net.Http;


namespace Rag.Core.Abstractions;

public sealed class EmbeddingGenerator : IEmbeddingGenerator
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EmbeddingGenerator(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public Task<float[]> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();
        throw new NotImplementedException();
    }
}