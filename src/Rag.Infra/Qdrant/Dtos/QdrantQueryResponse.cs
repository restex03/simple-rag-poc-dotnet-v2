using System.Text.Json.Serialization;

namespace Rag.Infra.Qdrant;

public sealed record QdrantQueryResponse
{

    [JsonPropertyName("result")]
    public QdrantQueryResult Result { get; init; }
}
