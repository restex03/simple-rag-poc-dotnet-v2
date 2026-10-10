using Rag.Core.Models;

namespace Rag.Core.Services;

public interface IRagService
{
    Task<RagAnswer> AnswerAsync(
        RagRequest request,
        CancellationToken cancellationToken);
    public Task IngestAsync(
        Document document,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<SearchResult>> SearchAsync(
        string query,
        int topK,
        double minimumScore,
        CancellationToken cancellationToken = default);

}