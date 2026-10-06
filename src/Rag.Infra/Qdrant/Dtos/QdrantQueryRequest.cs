using System.Text.Json.Serialization;

namespace Rag.Infra.Qdrant;

public sealed record QdrantQueryRequest
{

    [JsonPropertyName("query")]
    public float[] Query { get; init; }

    [JsonPropertyName("limit")]
    public int Limit { get; init; }

    [JsonPropertyName("with_payload")]
    public bool WithPayload { get; init; }
}
