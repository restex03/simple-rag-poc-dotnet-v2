


using Rag.Infra.Qdrant;
namespace Rag.Infra.Gateways;

public interface IQdrantGateway
{
    public Task UpsertAsync(
        string collectionName,
        QdrantUpsertRequest request,
        CancellationToken cancellationToken = default);
    Task<QdrantQueryResponse?> SearchAsync(
        string collectionName,
        QdrantQueryRequest request,
        CancellationToken cancellationToken = default);
}