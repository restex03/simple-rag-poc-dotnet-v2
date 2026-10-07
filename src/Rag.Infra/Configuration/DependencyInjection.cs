using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;
using Rag.Core.Abstractions;
using Rag.Infra.Gateways;
using Rag.Infra.OpenSearch;
using Rag.Infra.Qdrant;
using Rag.Infra.Services;

namespace Rag.Infra.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRagInfra(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // OpenSearch
        var openSearchOptions = configuration
            .GetRequiredSection(OpenSearchOptions.SectionName)
            .Get<OpenSearchOptions>()
            ?? throw new InvalidOperationException(
                "OpenSearch configuration is not found.");

        var openSearchSettings = new ConnectionSettings(
            new Uri(openSearchOptions.BaseAddress));

        services.AddSingleton<IOpenSearchClient>(
            new OpenSearchClient(openSearchSettings));

        services.AddSingleton<OpenSearchIndexInitializer>();

        services.AddSingleton<IDocumentIndex, OpenSearchDocumentIndex>();

        // Qdrant
        var qdrantOptions = configuration
            .GetRequiredSection(QdrantOptions.SectionName)
            .Get<QdrantOptions>()
            ?? throw new InvalidOperationException(
                "Qdrant configuration is not found.");

        services.AddHttpClient<IQdrantGateway, QdrantGateway>(client =>
        {
            client.BaseAddress = new Uri(qdrantOptions.BaseAddress);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddTransient<IVectorStore, QdrantVectorStore>();

        // Ollama
        var ollamaOptions = configuration
            .GetRequiredSection(OllamaOptions.SectionName)
            .Get<OllamaOptions>()
            ?? throw new InvalidOperationException(
                "Ollama configuration is not found.");

        services.AddHttpClient<IOllamaGateway, OllamaGateway>(client =>
        {
            client.BaseAddress = new Uri(ollamaOptions.BaseAddress);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddTransient<IEmbeddingGenerator, EmbeddingGenerator>();

        return services;
    }
}