using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;


using Rag.Core.Abstractions;
using Rag.Core.DependencyInjection;
using Rag.Core.Models;
using Rag.Core.Services;
using Rag.Infra.DependencyInjection;


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
        new Uri($"http://{qdrantOptions.Host}:{qdrantOptions.HttpPort}/healthz"),
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



app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});




await app.WaitForInfraReadiness(timeout: TimeSpan.FromSeconds(3));
await app.InitializeQdrant();
await app.InitializeOpenSearch();

/// <summary>
/// Ingest a new document.
/// </summary>
app.MapPost(
    "/documents",
    async (
        [FromBody] Document document,
        [FromServices] IRagService ragService,
        CancellationToken cancellationToken) =>
    {
        await ragService.IngestAsync(
            document,
            cancellationToken);

        return Results.Ok(new
        {
            document.Id,
            message = "Document indexed."
        });
    });

/// <summary>
/// Get document by ID.
/// </summary>
app.MapGet(
    "/documents/{id}",
    async (
        [FromRoute] string id,
        [FromServices] IDocumentIndex documentIndex,
        CancellationToken cancellationToken) =>
    {
        var document = await documentIndex.GetDocumentAsync(
            id,
            cancellationToken);

        return document is null
            ? Results.NotFound()
            : Results.Ok(document);
    });

/// <summary>
/// Search documents by query.string
/// </summary>
app.MapPost(
    "/search",
    async (
        [FromBody] SearchRequest request,
        [FromServices] IRagService ragService,
        CancellationToken cancellationToken) =>
    {
        var results = await ragService.SearchAsync(
            request.Query,
            request.TopK,
            request.minimumScore,
            cancellationToken);

        return Results.Ok(results);
    });

/// <summary>
/// Ask a question to the RAG system based on ingested documents.
/// </summary>
app.MapPost(
    "/ask",
    async (
        [FromBody] RagRequest request,
        [FromServices] IRagService ragService,
        CancellationToken cancellationToken) =>
    {

        var ragResult = await ragService.AnswerAsync(
            request,
            cancellationToken);

        return Results.Ok(ragResult);
    });



app.Run();









