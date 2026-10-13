namespace Rag.Core.Models;

public sealed record SearchAdjacentRequest(
    string DocumentId,
    int ChunkIndex,
    int Radius);