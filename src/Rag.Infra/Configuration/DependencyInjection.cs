using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Rag.Infra.Qdrant;
using Rag.Infra.Gateways;
using Rag.Core.Abstractions;

namespace Rag.Infra.DependencyInjection;

public static class ServiceCollectionExtensions
{
    // The 'this' keyword makes this method available on IServiceCollection instances
    public static IServiceCollection AddRagInfra(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IVectorStore, QdrantVectorStore>();
        services.AddSingleton<IOllamaGateway, OllamaGateway>();


        var qdrantOptions = configuration.GetRequiredSection(QdrantOptions.SectionName)
            .Get<QdrantOptions>() ?? throw new InvalidOperationException("Qdrant configuration is not found.");

        var ollamaOptions = configuration.GetRequiredSection(OllamaOptions.SectionName)
                    .Get<OllamaOptions>() ?? throw new InvalidOperationException("Ollama configuration is not found.");
        services.AddHttpClient<OllamaGateway>(client =>
        {
            client.BaseAddress = new Uri(ollamaOptions.BaseAddress);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("Qdrant", client =>
        {
            client.BaseAddress = new Uri(qdrantOptions.BaseAddress);
        });
        return services;
    }

}