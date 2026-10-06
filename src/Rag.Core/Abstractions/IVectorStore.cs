using Rag.Core.Models;

namespace Rag.Core.Abstractions;

public interface IVectorStore
{
    Task StoreAsync(
        DocumentChunk chunk,
        float[] embedding,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchResult>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default);


}
