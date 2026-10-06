using System.Net.Http.Json;

namespace Rag.Infra.Qdrant;

public sealed class QdrantGateway
{
    private const string ClientName = "Qdrant";

    private readonly IHttpClientFactory _httpClientFactory;

    public QdrantGateway(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task UpsertAsync(
        string collectionName,
        QdrantUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient(ClientName);

        var response = await client.PutAsJsonAsync(
            $"/collections/{collectionName}/points?wait=true",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<QdrantQueryResponse?> SearchAsync(
        string collectionName,
        QdrantQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient(ClientName);

        var response = await client.PostAsJsonAsync(
            $"/collections/{collectionName}/points/query",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QdrantQueryResponse>(
            cancellationToken);
    }
}