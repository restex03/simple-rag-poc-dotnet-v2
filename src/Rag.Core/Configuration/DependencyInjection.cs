using Microsoft.Extensions.DependencyInjection;
using Rag.Core.Services;
using Rag.Core.Chunking;
namespace Rag.Core.DependencyInjection;

public static class ServiceCollectionExtensions
    {
        // The 'this' keyword makes this method available on IServiceCollection instances
        public static IServiceCollection AddRagCore(this IServiceCollection services)
        {
            services.AddSingleton<IChunkingStrategy, CharacterChunkingStrategy>();
            services.AddSingleton<RagService>();
            return services;
        }
    }