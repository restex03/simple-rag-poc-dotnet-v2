namespace Rag.Infra.Gateways;

public interface IOllamaGateway
{
    Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);

    Task<string> CompleteAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}