using System.Text.Json;

using OpenAI.Chat;

using Rag.Core.Abstractions;
using Rag.Core.Models;

namespace Rag.Infra.Inference;

public sealed class OpenAiChatCompletionService : IChatCompletionService
{

    private readonly ChatClient _client;

    public OpenAiChatCompletionService(ChatClient client)
    {
        _client = client;
    }

    public async Task<string> CompleteAsync(RagPrompt prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prompt);

        var untrustedPayload = JsonSerializer.Serialize(new
        {
            prompt.Question,
            prompt.Context
        });

        ChatMessage[] messages =
        [
            new SystemChatMessage(prompt.Instructions),
            new UserChatMessage(untrustedPayload)
        ];
        var completion = await _client.CompleteChatAsync(
            messages,
            cancellationToken: cancellationToken);
        if (completion?.Value?.Content == null)
        {
            throw new InvalidOperationException("Chat completion returned no content.");
        }


        return string.Join(
            "\n\n",
            completion.Value.Content
                .Select(x => x.Text)
                .Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}