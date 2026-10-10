namespace Rag.Core.Models;

public sealed record SearchRequest(
    string Query,
    int TopK = 5,
    double minimumScore = 0.70d);