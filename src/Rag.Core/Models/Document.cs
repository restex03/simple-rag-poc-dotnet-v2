namespace Rag.Core.Models;

public sealed record Document(
    string Id,
    string Title,
    string Content);