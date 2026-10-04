namespace Rag.Core.Abstractions;

public interface IChatCompletionService
{
    Task<string> CompleteAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}