namespace Rag.Core.Models;

public sealed record DocumentChunk(
    string DocumentId,
    string ChunkId,
    string Text,
    int ChunkIndex);