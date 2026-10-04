using Rag.Core.Abstractions;
using Rag.Core.Chunking;

namespace Rag.Core.Services;
public sealed class RagService
{
    private readonly IChunkingStrategy ChunkingStrategy;
    private readonly IVectorStore VectorStore;
    private readonly IEmbeddingGenerator EmbeddingGenerator;
    private readonly IChatCompletionService ChatCompletionService;

    public RagService(IChunkingStrategy chunkingStrategy, IVectorStore vectorStore, IEmbeddingGenerator embeddingGenerator, IChatCompletionService chatCompletionService)
    {
        this.ChunkingStrategy = chunkingStrategy;
        this.VectorStore = vectorStore;
        this.EmbeddingGenerator = embeddingGenerator;
        this.ChatCompletionService = chatCompletionService;
    }

    public string GetStatus()
    {
        return "RAG service ready";
    }
}