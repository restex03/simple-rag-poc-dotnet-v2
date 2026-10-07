using Rag.Core.Abstractions;
using Rag.Infra.Gateways;

namespace Rag.Infra.Services;

public sealed class EmbeddingGenerator : IEmbeddingGenerator
{
    private readonly IOllamaGateway _ollamaGateway;

    public EmbeddingGenerator(IOllamaGateway ollamaGateway)
    {
        _ollamaGateway = ollamaGateway;
    }

    public Task<float[]> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        return _ollamaGateway.GenerateEmbeddingAsync(
            text,
            cancellationToken);
    }
}