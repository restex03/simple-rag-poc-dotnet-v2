using OpenSearch.Client;

namespace Rag.Infra.OpenSearch;

public sealed class OpenSearchIndexInitializer
{
    private readonly IOpenSearchClient _client;

    public OpenSearchIndexInitializer(IOpenSearchClient client)
    {
        _client = client;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureDocumentsIndexAsync(cancellationToken);
        await EnsureDocumentChunksIndexAsync(cancellationToken);
    }

    private async Task EnsureDocumentsIndexAsync(
        CancellationToken cancellationToken)
    {
        var exists = await _client.Indices.ExistsAsync(
            "documents",
            ct: cancellationToken);

        if (exists.Exists)
            return;

        var response = await _client.Indices.CreateAsync(
            "documents",
            x => x.Map<DocumentIndexModel>(m => m
                .Properties(p => p
                    .Keyword(k => k.Name(n => n.Id))
                    .Text(t => t.Name(n => n.Title))
                    .Text(t => t.Name(n => n.Content)))),
            cancellationToken);

        if (!response.IsValid)
            throw new InvalidOperationException("Failed to create documents index.");
    }

    private async Task EnsureDocumentChunksIndexAsync(
        CancellationToken cancellationToken)
    {
        var exists = await _client.Indices.ExistsAsync(
            "document-chunks",
            ct: cancellationToken);

        if (exists.Exists)
            return;

        var response = await _client.Indices.CreateAsync(
            "document-chunks",
            x => x
                .Settings(s => s
                    .Setting("index.knn", true))
                .Map<DocumentChunkIndexModel>(m => m
                    .Properties(p => p
                        .Keyword(k => k.Name(n => n.DocumentId))
                        .Keyword(k => k.Name(n => n.ChunkId))
                        .Text(t => t.Name(n => n.Text))
                        .Number(n => n
                            .Name(x => x.ChunkIndex)
                            .Type(NumberType.Integer))
                        .KnnVector(v => v
                            .Name(n => n.Embedding)
                            .Dimension(1024)))),
            cancellationToken);

        if (!response.IsValid)
            throw new InvalidOperationException(
                "Failed to create document-chunks index.");
    }

    private sealed class DocumentIndexModel
    {
        public string Id { get; init; } = "";
        public string Title { get; init; } = "";
        public string Content { get; init; } = "";
    }

    private sealed class DocumentChunkIndexModel
    {
        public string DocumentId { get; init; } = "";
        public string ChunkId { get; init; } = "";
        public string Text { get; init; } = "";
        public int ChunkIndex { get; init; }
        public float[] Embedding { get; init; } = [];
    }
}