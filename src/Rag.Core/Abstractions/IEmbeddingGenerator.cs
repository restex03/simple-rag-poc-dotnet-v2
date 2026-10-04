namespace Rag.Core.Abstractions;

public interface IEmbeddingGenerator
{
    Task<float[]> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default);
}