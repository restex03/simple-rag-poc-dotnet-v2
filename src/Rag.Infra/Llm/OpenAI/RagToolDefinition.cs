using OpenAI.Chat;

public static class RagToolDefinitions
{


    public static readonly ChatTool SearchTool = ChatTool.CreateFunctionTool(
        functionName: "search_tool",
        functionDescription: "Search indexed documents for information relevant to the user's question.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "query": {
                    "type": "string",
                    "description": "Semantic search query"
                },
                "topK": {
                    "type": "integer",
                    "description": "Maximum number of results to retrieve"
                }
            },
            "required": ["query", "topK"],
            "additionalProperties": false
        }
        """)
    );

    public static readonly ChatTool SearchAdjacentTool = ChatTool.CreateFunctionTool(
        functionName: "search_adjacent_chunks",
        functionDescription: "Retrieve adjacent chunks for a given chunk when the given chunk lacks clarity and needs additional context",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "documentId": {
                    "type": "string",
                    "description": "document ID from which to retrieve adjacent chunks"
                },
                "chunkIndex": {
                    "type": "integer",
                    "description": "Index of chunk to pull adjacent chunks"
                },
                "radius": {
                    "type": "integer",
                    "description": "Number of neighboring chunks to retrieve on each side"
                }
            },
            "required": ["documentId", "chunkIndex", "radius"],
            "additionalProperties": false
        }
        """)
    );

    public static IReadOnlyList<ChatTool> All { get; } = [
      SearchTool,
      SearchAdjacentTool
    ];

}