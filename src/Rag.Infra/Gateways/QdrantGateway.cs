using System.Net.Http.Json;

using Rag.Infra.Qdrant;

namespace Rag.Infra.Gateways;

public sealed class QdrantGateway : IQdrantGateway
{
    private const string ClientName = "Qdrant";

    private readonly HttpClient _httpClient;

    public QdrantGateway(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task UpsertAsync(
        string collectionName,
        QdrantUpsertRequest request,
        CancellationToken cancellationToken = default)
    {


        var response = await _httpClient.PutAsJsonAsync(
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

        var response = await _httpClient.PostAsJsonAsync(
            $"/collections/{collectionName}/points/query",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QdrantQueryResponse>(
            cancellationToken);
    }
}