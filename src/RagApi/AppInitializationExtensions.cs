using Qdrant.Client;
using Qdrant.Client.Grpc;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using Rag.Infra.OpenSearch;


public static class AppInitializationExtensions
{


    public static async Task WaitForInfraReadiness(
    this WebApplication app,
    TimeSpan timeout,
    CancellationToken cancellationToken = default)
    {
        var healthChecks = app.Services
            .GetRequiredService<HealthCheckService>();

        using var cts = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken);

        cts.CancelAfter(timeout);

        HealthReport? lastReport = null;

        try
        {
            while (true)
            {
                cts.Token.ThrowIfCancellationRequested();

                lastReport = await healthChecks.CheckHealthAsync(
                    x => x.Tags.Contains("ready"),
                    cts.Token);

                if (lastReport.Status == HealthStatus.Healthy)
                    return;

                await Task.Delay(500, cts.Token);
            }
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            var failures = lastReport?.Entries
                .Where(x => x.Value.Status != HealthStatus.Healthy)
                .Select(x =>
                    $"{x.Key}: {x.Value.Exception?.GetBaseException().Message
                        ?? x.Value.Description
                        ?? x.Value.Status.ToString()}")
                .ToArray() ?? [];

            throw new TimeoutException(
                $"\n\t[WaitForInfraReadiness] Infrastructure readiness timeout after {timeout.TotalSeconds}s. " +
                $"\n\t[WaitForInfraReadiness] Unhealthy dependencies -> {string.Join("; ", failures)}");
        }
    }

    public static async Task InitializeQdrant(this WebApplication app)
    {
        const string collectionName = "documents";

        using var scope = app.Services.CreateScope();

        var qdrantClient = scope.ServiceProvider.GetRequiredService<QdrantClient>();

        var collectionExists = await qdrantClient.CollectionExistsAsync(collectionName);
        if (collectionExists)
        {
            return;
        }

        qdrantClient.CreateCollectionAsync(
            collectionName: collectionName,
            vectorsConfig: new VectorParams
            {
                Size = 2560,
                Distance = Distance.Cosine
            }).GetAwaiter().GetResult();

    }

    public static async Task InitializeOpenSearch(this WebApplication app)
    {
        var initializer = app.Services.GetRequiredService<OpenSearchIndexInitializer>();

        await initializer.InitializeAsync();
    }
}