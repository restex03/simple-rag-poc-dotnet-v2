namespace Rag.Core.Models;

public sealed record SearchResult(
    DocumentChunk Chunk,
    double Score);