public interface IOllamaGateway
{
    Task<string> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);

    Task<string> CompleteAsync(
        string prompt,
        CancellationToken cancellationToken = default);

}