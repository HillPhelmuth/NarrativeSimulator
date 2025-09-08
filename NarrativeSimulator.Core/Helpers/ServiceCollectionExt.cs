using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NarrativeSimulator.Core.Services;

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
    public static IServiceCollection AddSentinoClient(this IServiceCollection services, Action<SentinoOptions> configure)
    {
        services.Configure(configure);
        services.AddHttpClient<ISentinoClient, SentinoClient>((sp, http) =>
        {
            var opts = sp.GetRequiredService<IOptions<SentinoOptions>>().Value;
            http.BaseAddress = new Uri(opts.BaseUrl);
            // If RapidAPI requires host header for your plan, uncomment next line:
            // http.DefaultRequestHeaders.TryAddWithoutValidation("x-rapidapi-host", "sentino.p.rapidapi.com");
        });
        return services;
    }
    public static IServiceCollection AddSymantoClient(this IServiceCollection services, Action<SymantoOptions> configure)
    {
        services.Configure(configure);
        services.AddHttpClient<ISymantoClient, SymantoClient>((sp, http) =>
        {
            var opts = sp.GetRequiredService<IOptions<SymantoOptions>>().Value;
            http.BaseAddress = new Uri(opts.BaseUrl);
            // If RapidAPI requires host header for your plan, uncomment next line:
            // http.DefaultRequestHeaders.TryAddWithoutValidation("x-rapidapi-host", "symanto-text-analysis.p.rapidapi.com");
        });
        return services;
    }
}