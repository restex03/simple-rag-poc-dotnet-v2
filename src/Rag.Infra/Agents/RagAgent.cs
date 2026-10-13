using System.Net.Http.Headers;

using OpenAI.Chat;

public sealed class RagAgent
{
    private readonly ChatClient _chatClient;
    private readonly RagToolDispatcher _toolDispatcher;
    private readonly ChatCompletionOptions _options;

    public RagAgent(
        ChatClient chatClient,
        RagToolDispatcher toolDispatcher,
        IEnumerable<ChatTool> tools)
    {
        _chatClient = chatClient;
        _toolDispatcher = toolDispatcher;

        _options = new ChatCompletionOptions();

        foreach (var tool in tools)
        {
            _options.Tools.Add(tool);
        }
    }

    public async Task<string> RunAsync(
        string question,
        CancellationToken ct = default)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("""
                You are a document research assistant.

                Use the available tools to retrieve evidence.
                If a search result appears relavant but is missing important context, 
                retrieve adjacent chunks. 

                If retrieving adjacent chunks still does not provide grounded evidence,
                simply respond "I don't have enough evidence to confidently answer the question."

                Do not invent facts.
                Answer using the retrieved evidence.
            """),

            new UserChatMessage(question)
        };

        const int maxIterations = 8;

        for (var iteration = 0; iteration < maxIterations; ++iteration)
        {
            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, _options, ct);

            messages.Add(new AssistantChatMessage(completion));

            if (completion.FinishReason == ChatFinishReason.Stop)
            {
                return string.Concat(completion.Content.Select(part => part.Text));
            }

            if (completion.FinishReason != ChatFinishReason.ToolCalls)
            {
                var errMessage = $"Unexpected finish reason: {completion.FinishReason}";
                throw new InvalidOperationException(errMessage);
            }

            foreach (var toolCall in completion.ToolCalls)
            {
                var result = await _toolDispatcher.ExecuteAsync(toolCall, ct);

                messages.Add(new ToolChatMessage(toolCall.Id, result));
            }
        }

        throw new InvalidOperationException("Agent exceeded its iteration limit.");
    }
}