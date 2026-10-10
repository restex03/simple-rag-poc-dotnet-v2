public sealed record RagRequest(
    string Question,
    int TopK = 5,
    double MinimumScore = 0.70d);