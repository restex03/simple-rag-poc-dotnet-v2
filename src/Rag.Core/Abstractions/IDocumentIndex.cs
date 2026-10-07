using Rag.Core.Models;

namespace Rag.Core.Abstractions;

public interface IDocumentIndex
{
    Task IndexDocumentAsync(
        Document document,
        CancellationToken cancellationToken = default);

    Task<Document?> GetDocumentAsync(
        string documentId,
        CancellationToken cancellationToken = default);
}