using OpenAI.Chat;

using Rag.Core.Abstractions;

namespace Rag.Infra.Inference;

public sealed class OpenAiChatCompletionService : IChatCompletionService
{

    private readonly ChatClient _client;

    public OpenAiChatCompletionService(ChatClient client)
    {
        _client = client;
    }

    public async Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var completion = await _client.CompleteChatAsync(prompt);
        if (completion?.Value?.Content == null)
        {
            throw new InvalidOperationException("Chat completion returned no content.");
        }
        return string.Join(" ", completion.Value.Content.Select(x => x.Text));
    }
}