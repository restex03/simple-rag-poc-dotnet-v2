using Rag.Core.Abstractions;

public sealed class ChatCompletionService : IChatCompletionService
{
    public Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}