using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hipsy.Net;

/// <summary>Registers <see cref="IHipsyClient"/> with dependency injection.</summary>
public static class HipsyServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IHipsyClient"/> as a typed <see cref="HttpClient"/>, with options set in code.
    /// The returned builder lets you add handlers, for example a resilience/retry handler for 429 responses.
    /// </summary>
    public static IHttpClientBuilder AddHipsyClient(this IServiceCollection services, Action<HipsyClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<HipsyClientOptions>().Configure(configure);
        return services.AddHttpClient<IHipsyClient, HipsyClient>();
    }

    /// <summary>
    /// Registers <see cref="IHipsyClient"/> as a typed <see cref="HttpClient"/>, with options bound from
    /// configuration (by default the <c>Hipsy</c> section, for example <c>Hipsy:ApiKey</c>).
    /// </summary>
    public static IHttpClientBuilder AddHipsyClient(this IServiceCollection services, IConfiguration configuration, string sectionName = HipsyClientOptions.SectionName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<HipsyClientOptions>().Bind(configuration.GetSection(sectionName));
        return services.AddHttpClient<IHipsyClient, HipsyClient>();
    }
}
