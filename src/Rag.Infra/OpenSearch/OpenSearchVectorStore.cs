using OpenSearch.Client;

using Rag.Core.Abstractions;
using Rag.Core.Models;

namespace Rag.Infra.OpenSearch;

public sealed class OpenSearchDocumentIndex : IDocumentIndex
{
    private const string IndexName = "documents";

    private readonly IOpenSearchClient _client;

    public OpenSearchDocumentIndex(IOpenSearchClient client)
    {
        _client = client;
    }

    public async Task IndexDocumentAsync(
        Document document,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.IndexAsync(
            document,
            x => x
                .Index(IndexName)
                .Id(document.Id),
            cancellationToken);

        if (!response.IsValid)
        {
            throw new InvalidOperationException(
                response.ServerError?.ToString()
                ?? response.OriginalException?.Message
                ?? "Failed to index document.");
        }
    }

    public async Task<Document?> GetDocumentAsync(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        var response = await _client.GetAsync<Document>(
            documentId,
            x => x.Index(IndexName),
            cancellationToken);

        if (!response.IsValid)
        {
            if (response.ApiCall?.HttpStatusCode == 404)
            {
                return null;
            }

            throw new InvalidOperationException(
                response.ServerError?.ToString()
                ?? response.OriginalException?.Message
                ?? "Failed to retrieve document.");
        }

        return response.Found
            ? response.Source
            : null;
    }
}