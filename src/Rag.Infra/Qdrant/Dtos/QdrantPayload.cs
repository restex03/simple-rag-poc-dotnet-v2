using System.Text.Json.Serialization;

namespace Rag.Infra.Qdrant;

public sealed record QdrantPayload
{

    [JsonPropertyName("documentId")]
    public string DocumentId { get; init; }

    [JsonPropertyName("chunkId")]
    public string ChunkId { get; init; }

    [JsonPropertyName("text")]
    public string Text { get; init; }

    [JsonPropertyName("chunkIndex")]
    public int ChunkIndex { get; init; }
}
