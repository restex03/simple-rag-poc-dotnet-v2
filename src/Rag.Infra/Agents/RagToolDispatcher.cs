using System.Text.Json;

using OpenAI.Chat;

using Rag.Core.Abstractions;
using Rag.Core.Services;


public sealed class RagToolDispatcher
{
    private readonly IRagService _ragService;

    public RagToolDispatcher(IRagService ragService)
    {
        _ragService = ragService;
    }

    public async Task<string> ExecuteAsync(
        ChatToolCall toolCall,
        CancellationToken ct)
    {
        using var args = JsonDocument.Parse(toolCall.FunctionArguments.ToString());

        var root = args.RootElement;

        object result = toolCall.FunctionName switch
        {
            "search_tool" => await _ragService.SearchAsync(
                query: root.GetProperty("query").GetString()!,
                topK: root.GetProperty("topK").GetInt32(),
                minimumScore: 0.4,
                cancellationToken: ct
            ),

            "search_adjacent_chunks" => await _ragService.SearchAdjacentAsync(
                documentId: root.GetProperty("documentId").GetString()!,
                chunkIndex: root.GetProperty("chunkIndex").GetInt32(),
                radius: root.GetProperty("radius").GetInt32()
            ),

            _ => throw new InvalidOperationException($"Unsupported tool: {toolCall.FunctionName}")
        };

        return JsonSerializer.Serialize(result);
    }

}