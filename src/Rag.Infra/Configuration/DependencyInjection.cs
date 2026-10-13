using System.ClientModel;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenAI;
using OpenAI.Chat;

using OpenSearch.Client;

using Rag.Core.Abstractions;
using Rag.Infra.Gateways;
using Rag.Infra.OpenSearch;
using Rag.Infra.Vector;
using Rag.Infra.Services;
using Rag.Infra.Inference;

using Qdrant.Client;
using System.ClientModel.Primitives;

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
        services.AddSingleton(new QdrantClient(qdrantOptions.Host, qdrantOptions.GrpcPort));
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


        var chatClientOptions = configuration.GetRequiredSection(ChatClientOptions.SectionName)
            .Get<ChatClientOptions>()
            ?? throw new InvalidOperationException(
                "Chat client configuration is not found.");

        services.AddSingleton(sp =>
        {

            var handler = new OpenAiHttpLoggingHandler
            {
                InnerHandler = new HttpClientHandler()
            };

            var httpClient = new HttpClient(handler);

            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri($"{chatClientOptions.BaseAddress.TrimEnd('/')}/v1"),

                Transport = new HttpClientPipelineTransport(httpClient)
            };

            var result = new ChatClient(
                model: chatClientOptions.Model,
                credential: new ApiKeyCredential(chatClientOptions.ApiKey),
                options: options);

            return result;
        });


        services.AddSingleton<
            IChatCompletionService,
            OpenAiChatCompletionService>();

        services.AddScoped<RagToolDispatcher>();
        services.AddScoped<RagAgent>(sp =>
            new RagAgent(
                sp.GetRequiredService<ChatClient>(),
                sp.GetRequiredService<RagToolDispatcher>(),
                RagToolDefinitions.All));

        return services;
    }
}