namespace Rag.Core.Models;

public sealed record RagPrompt(
    string Instructions,
    string Question,
    IReadOnlyList<RagContext> Context);

public sealed record RagContext(
    string DocumentId,
    int ChunkIndex,
    string Content);
