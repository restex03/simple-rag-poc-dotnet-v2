
using System.Security.Cryptography;
using System.Text;

using Qdrant.Client;
using Qdrant.Client.Grpc;

using Rag.Core.Abstractions;
using Rag.Core.Models;

using static Qdrant.Client.Grpc.Conditions;

namespace Rag.Infra.Vector;

public sealed class QdrantVectorStore : IVectorStore
{
    private const string CollectionName = "documents";

    private readonly QdrantClient _client;

    public QdrantVectorStore(QdrantClient client)
    {
        _client = client;
    }

    public async Task StoreAsync(
        DocumentChunk chunk,
        float[] embedding,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(embedding);

        if (embedding.Length == 0)
            throw new ArgumentException(
                "Embedding cannot be empty.", nameof(embedding));

        var point = new PointStruct
        {
            Id = CreatePointId(chunk),
            Vectors = embedding,
            Payload =
            {
                [QdrantPayloadFields.DocumentId] = chunk.DocumentId,
                [QdrantPayloadFields.ChunkId] = chunk.ChunkId,
                [QdrantPayloadFields.ChunkIndex] = chunk.ChunkIndex,
                [QdrantPayloadFields.Text] = chunk.Text
            }
        };

        await _client.UpsertAsync(
            collectionName: CollectionName,
            points: new[] { point },
            wait: true,
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        double minimumScore,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queryEmbedding);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topK);

        if (queryEmbedding.Length == 0)
            throw new ArgumentException(
                "Query embedding cannot be empty.",
                nameof(queryEmbedding));


        var results = await _client.QueryAsync(
            collectionName: CollectionName,
            query: queryEmbedding,
            limit: (ulong)topK,
            scoreThreshold: (float)minimumScore,
            payloadSelector: true,
            vectorsSelector: false,
            cancellationToken: cancellationToken);

        return results
            .Select(point => new SearchResult(
                ToDocumentChunk(point.Payload),
                point.Score))
            .ToList();
    }

    public async Task<IReadOnlyList<DocumentChunk>> GetChunksAsync(
        string documentId,
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        if (startIndex < 0 || endIndex < startIndex)
            throw new ArgumentOutOfRangeException(
                nameof(startIndex),
                "Invalid chunk index range.");

        var filter =
            MatchKeyword(QdrantPayloadFields.DocumentId, documentId)
            & Range(
                QdrantPayloadFields.ChunkIndex,
                new Qdrant.Client.Grpc.Range
                {
                    Gte = startIndex,
                    Lte = endIndex
                });

        var chunks = new List<DocumentChunk>();
        PointId? offset = null;

        do
        {
            var page = await _client.ScrollAsync(
                collectionName: CollectionName,
                filter: filter,
                limit: 100,
                offset: offset,
                payloadSelector: true,
                vectorsSelector: false,
                cancellationToken: cancellationToken);

            chunks.AddRange(
                page.Result.Select(p => ToDocumentChunk(p.Payload)));

            offset = page.NextPageOffset;

        } while (offset is not null);

        return chunks
            .OrderBy(c => c.ChunkIndex)
            .ToList();
    }

    private static DocumentChunk ToDocumentChunk(
        IReadOnlyDictionary<string, Value> payload)
    {
        return new DocumentChunk(
            DocumentId: payload[QdrantPayloadFields.DocumentId].StringValue,
            ChunkId: payload[QdrantPayloadFields.ChunkId].StringValue,
            Text: payload[QdrantPayloadFields.Text].StringValue,
            ChunkIndex: checked((int)payload[QdrantPayloadFields.ChunkIndex].IntegerValue));
    }

    private static Guid CreatePointId(DocumentChunk chunk)
    {
        // Stable UUID so repeated ingestion of the same
        // document/chunk overwrites rather than duplicates.
        var input = Encoding.UTF8.GetBytes(
            $"{chunk.DocumentId}:{chunk.ChunkId}");

        var hash = SHA256.HashData(input);
        var guidBytes = hash[..16];

        // Mark as a name-based UUID.
        guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x50);
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        return new Guid(guidBytes);
    }

}
