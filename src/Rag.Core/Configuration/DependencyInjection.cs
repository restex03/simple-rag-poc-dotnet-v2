using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Rag.Core.Services;
using Rag.Core.Chunking;
using Rag.Core.Abstractions;

namespace Rag.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    // The 'this' keyword makes this method available on IServiceCollection instances
    public static IServiceCollection AddRagCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IChunkingStrategy, CharacterChunkingStrategy>();
        services.AddSingleton<IRagService, RagService>();

        return services;
    }
}