namespace Rag.Core.Models;

public sealed record RetrievalRequest(
    string Query,
    int TopK,
    double MinimumScore);