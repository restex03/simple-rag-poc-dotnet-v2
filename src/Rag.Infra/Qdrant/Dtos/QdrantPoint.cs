using System.Text.Json.Serialization;

namespace Rag.Infra.Qdrant;

public sealed record QdrantPoint
{

    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("vector")]
    public float[] Vector { get; init; }

    [JsonPropertyName("payload")]
    public QdrantPayload Payload { get; init; }
}
