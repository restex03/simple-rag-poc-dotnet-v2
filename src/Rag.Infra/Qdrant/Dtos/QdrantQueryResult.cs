using System.Text.Json.Serialization;

namespace Rag.Infra.Qdrant;

public sealed record QdrantQueryResult
{

    [JsonPropertyName("points")]
    public IReadOnlyList<QdrantQueryPoint> Points { get; init; }
}
