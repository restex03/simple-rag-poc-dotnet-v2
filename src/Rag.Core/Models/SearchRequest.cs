namespace Rag.Core.Models;

public sealed record SearchRequest(
    string Query,
    int TopK = 5,
    double MinimumScore = 0.70d);