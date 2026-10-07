using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Rag.Core.Abstractions;
using Rag.Core.DependencyInjection;
using Rag.Core.Models;
using Rag.Infra.DependencyInjection;
using Rag.Infra.OpenSearch;

var builder = WebApplication.CreateBuilder(args);

var ollamaOptions = builder.Configuration
    .GetRequiredSection(OllamaOptions.SectionName)
    .Get<OllamaOptions>()
    ?? throw new InvalidOperationException(
        "Ollama configuration is not found.");

var qdrantOptions = builder.Configuration
    .GetRequiredSection(QdrantOptions.SectionName)
    .Get<QdrantOptions>()
    ?? throw new InvalidOperationException(
        "Qdrant configuration is not found.");

var openSearchOptions = builder.Configuration
    .GetRequiredSection(OpenSearchOptions.SectionName)
    .Get<OpenSearchOptions>()
    ?? throw new InvalidOperationException(
        "OpenSearch configuration is not found.");

builder.Services
    .AddHealthChecks()
    .AddUrlGroup(
        new Uri($"{ollamaOptions.BaseAddress}/api/tags"),
        name: "ollama",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"])
    .AddUrlGroup(
        new Uri($"{qdrantOptions.BaseAddress}/healthz"),
        name: "qdrant",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"])
    .AddUrlGroup(
        new Uri(openSearchOptions.BaseAddress),
        name: "opensearch",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]);

builder.Services.AddRagCore(builder.Configuration);
builder.Services.AddRagInfra(builder.Configuration);

var app = builder.Build();

var initializer =
    app.Services.GetRequiredService<OpenSearchIndexInitializer>();

await initializer.InitializeAsync();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapPost(
    "/documents",
    async (
        Document document,
        IDocumentIndex documentIndex,
        CancellationToken cancellationToken) =>
    {
        await documentIndex.IndexDocumentAsync(
            document,
            cancellationToken);

        return Results.Ok(new
        {
            document.Id,
            message = "Document indexed."
        });
    });

app.MapPost(
    "/search",
    async (
        SearchRequest request,
        IEmbeddingGenerator embeddingGenerator,
        IVectorStore vectorStore,
        CancellationToken cancellationToken) =>
    {
        var embedding = await embeddingGenerator.GenerateAsync(
            request.Query,
            cancellationToken);

        var results = await vectorStore.SearchAsync(
            embedding,
            request.TopK,
            cancellationToken);

        return Results.Ok(results);
    });

app.MapPost(
    "/ask",
    async (
        AskRequest request,
        IEmbeddingGenerator embeddingGenerator,
        IVectorStore vectorStore,
        CancellationToken cancellationToken) =>
    {
        var embedding = await embeddingGenerator.GenerateAsync(
            request.Question,
            cancellationToken);

        var results = await vectorStore.SearchAsync(
            embedding,
            request.TopK,
            cancellationToken);

        return Results.Ok(results);
    });


app.MapGet(
    "/documents/{id}",
    async (
        string id,
        IDocumentIndex documentIndex,
        CancellationToken cancellationToken) =>
    {
        var document = await documentIndex.GetDocumentAsync(
            id,
            cancellationToken);

        return document is null
            ? Results.NotFound()
            : Results.Ok(document);
    });

app.Run();

public sealed record SearchRequest(
    string Query,
    int TopK = 5);

public sealed record AskRequest(
    string Question,
    int TopK = 5);