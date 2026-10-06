using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;


using Rag.Core.DependencyInjection;
using Rag.Infra.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var ollamaOptions = builder.Configuration.GetRequiredSection(OllamaOptions.SectionName)
            .Get<OllamaOptions>() ?? throw new InvalidOperationException("Ollama configuration is not found.");
var qdrantOptions = builder.Configuration.GetRequiredSection(QdrantOptions.SectionName)
            .Get<QdrantOptions>() ?? throw new InvalidOperationException("Qdrant configuration is not found.");

builder.Services.AddHealthChecks()
    .AddUrlGroup(
        new Uri($"{ollamaOptions.BaseAddress}/api/tags"),
        name: "ollama",
        tags: ["ready"])
    .AddUrlGroup(
        new Uri($"{qdrantOptions.BaseAddress}/healthz"),
        name: "qdrant",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]);


builder.Services.AddRagCore(builder.Configuration);
builder.Services.AddRagInfra(builder.Configuration);

var app = builder.Build();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => true
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapPost("/documents", () =>
{
    return Results.Ok(new
    {
        message = "Document ingestion endpoint",
    });
});

app.MapPost("/search", () =>
{
    return Results.Ok(new
    {
        message = "Vector Search endpoint",
    });
});

app.MapPost("/ask", () =>
{
    return Results.Ok(new
    {
        message = "RAG question answering endpoint",
    });
});



app.Run();
