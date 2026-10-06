using System.Text.Json.Serialization;

namespace Rag.Infra.Qdrant;

public sealed record QdrantQueryPoint
{

    [JsonPropertyName("score")]
    public double Score { get; init; }

    [JsonPropertyName("payload")]
    public QdrantPayload Payload { get; init; }
}