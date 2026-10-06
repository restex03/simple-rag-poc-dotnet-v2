using System.Text.Json.Serialization;

namespace Rag.Infra.Qdrant;

public sealed record QdrantUpsertRequest
{

    [JsonPropertyName("points")]
    public IReadOnlyList<QdrantPoint> Points { get; init; }
}
