using Microsoft.Extensions.DependencyInjection;
using NarrativeSimulator.Core.Services;
// For AiAgentOrchestration, BeatEngine, IBeatEngine

// For INarrativeOrchestration, NarrativeOrchestration, WorldState

namespace NarrativeSimulator.Core.Helpers;

public static class ServiceCollectionExt
{
    public static IServiceCollection AddNarrativeServices(this IServiceCollection services)
    {
        // Register narrative-related services here
        services.AddSingleton<WorldState>();
        services.AddScoped<INarrativeOrchestration, NarrativeOrchestration>();
        services.AddScoped<IBeatEngine, BeatEngine>();
        return services;
    }
}