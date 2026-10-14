using System.Net.Http.Headers;
using System.Text.Json;

using OpenAI.Chat;

public sealed class RagAgent
{
    private readonly ChatClient _chatClient;
    private readonly RagToolDispatcher _toolDispatcher;
    private readonly IReadOnlyList<ChatTool> _tools;


    public RagAgent(
        ChatClient chatClient,
        RagToolDispatcher toolDispatcher,
        IEnumerable<ChatTool> tools)
    {
        _chatClient = chatClient;
        _toolDispatcher = toolDispatcher;
        _tools = tools.ToList();
    }

    public async Task<string> RunAsync(
        string question,
        CancellationToken ct = default)
    {
        var messages = new List<ChatMessage>
        {


            new SystemChatMessage("""
                You are a personal research assistant with access to documents as well as chat transcripts.
                Before doing anything, call search_tool with the User's question. 

                Provide answers to the question using available tools search_tool, and search_adjacent_chunks. 
                
                Use search_tool to search through documents (including chat transcripts) for answers to the 
                user's question. 

                Use search_adjacent_chunks if a result from search_tool appears relavant but lacks context. 

                Do not provide a final answer until you have received and examined the tool results.

                Do not provide a final answer until you are confident in your findings. 

                If you have already exhausted calls to search_tool and search_adjacent_chunks, and still
                do not have enough context to confidently answer the question, 
                you may reply "I don't have enough evidence to confidently answer the question."
            """),

            new UserChatMessage(question),

        };

        const int maxIterations = 8;

        for (var iteration = 0; iteration < maxIterations; ++iteration)
        {

            var options = new ChatCompletionOptions
            {
                ToolChoice = iteration == 0
                        ? ChatToolChoice.CreateRequiredChoice()
                        : ChatToolChoice.CreateAutoChoice(),
                MaxOutputTokenCount = 512,
                Temperature = 0.2f
            };

            foreach (var tool in _tools)
                options.Tools.Add(tool);

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options, ct);

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