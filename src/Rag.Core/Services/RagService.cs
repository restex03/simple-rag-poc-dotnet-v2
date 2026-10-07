using Rag.Core.Abstractions;
using Rag.Core.Chunking;
using Rag.Core.Models;

namespace Rag.Core.Services;

public sealed class RagService
{
    private readonly IChunkingStrategy _chunkingStrategy;
    private readonly IVectorStore _vectorStore;
    private readonly IDocumentIndex _documentIndex;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly IChatCompletionService _chatCompletionService;

    public RagService(
        IChunkingStrategy chunkingStrategy,
        IVectorStore vectorStore,
        IDocumentIndex documentIndex,
        IEmbeddingGenerator embeddingGenerator,
        IChatCompletionService chatCompletionService)
    {
        _chunkingStrategy = chunkingStrategy;
        _vectorStore = vectorStore;
        _documentIndex = documentIndex;
        _embeddingGenerator = embeddingGenerator;
        _chatCompletionService = chatCompletionService;
    }

    public string GetStatus()
    {
        return "RAG service ready";
    }

    public async Task IngestAsync(
        Document document,
        CancellationToken cancellationToken = default)
    {
        await _documentIndex.IndexDocumentAsync(
            document,
            cancellationToken);

        var chunks = _chunkingStrategy.Chunk(document);

        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingGenerator.GenerateAsync(
                chunk.Text,
                cancellationToken);

            await _vectorStore.StoreAsync(
                chunk,
                embedding,
                cancellationToken);
        }
    }

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var queryEmbedding = await _embeddingGenerator.GenerateAsync(
            query,
            cancellationToken);

        return await _vectorStore.SearchAsync(
            queryEmbedding,
            topK,
            cancellationToken);
    }

    public async Task TestVectorStore(
        CancellationToken cancellationToken = default)
    {
        var document = new Document(
            "1",
            "Test document",
            "The quick brown fox jumps over the lazy dog.");

        await IngestAsync(
            document,
            cancellationToken);

        var results = await SearchAsync(
            "What does the fox jump over?",
            topK: 3,
            cancellationToken);

        foreach (var result in results)
        {
            Console.WriteLine(
                $"Score: {result.Score:F4} | " +
                $"Chunk: {result.Chunk.Text}");
        }
    }
}