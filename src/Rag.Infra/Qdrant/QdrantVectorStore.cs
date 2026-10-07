using System.Security.Cryptography;
using System.Text;
using Rag.Core.Abstractions;
using Rag.Core.Models;
using Rag.Infra.Gateways;

namespace Rag.Infra.Qdrant;

public sealed class QdrantVectorStore : IVectorStore
{
    private const string CollectionName = "document-chunks";

    private readonly IQdrantGateway _gateway;

    public QdrantVectorStore(IQdrantGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task StoreAsync(
        DocumentChunk chunk,
        float[] embedding,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(embedding);

        var point = new QdrantPoint
        {
            Id = CreatePointId(chunk.ChunkId),

            Vector = embedding,

            Payload = new QdrantPayload
            {
                DocumentId = chunk.DocumentId,
                ChunkId = chunk.ChunkId,
                Text = chunk.Text,
                ChunkIndex = chunk.ChunkIndex
            }
        };

        var request = new QdrantUpsertRequest
        {
            Points = [point]
        };

        await _gateway.UpsertAsync(
            CollectionName,
            request,
            cancellationToken);
    }

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queryEmbedding);

        if (topK <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(topK),
                "topK must be greater than zero.");
        }

        var request = new QdrantQueryRequest
        {
            Query = queryEmbedding,
            Limit = topK,
            WithPayload = true
        };

        var response = await _gateway.SearchAsync(
            CollectionName,
            request,
            cancellationToken);

        if (response?.Result?.Points is null)
            return [];

        return response.Result.Points
            .Select(point => new SearchResult(
                new DocumentChunk(
                    point.Payload.DocumentId,
                    point.Payload.ChunkId,
                    point.Payload.Text,
                    point.Payload.ChunkIndex),
                point.Score))
            .ToArray();
    }

    private static Guid CreatePointId(string chunkId)
    {
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(chunkId));

        return new Guid(hash[..16]);
    }
}