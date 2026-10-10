using Rag.Core.Models;

namespace Rag.Core.Abstractions;

public interface IChatCompletionService
{
    Task<string> CompleteAsync(
        RagPrompt prompt,
        CancellationToken cancellationToken = default);
}