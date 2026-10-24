using Hipsy.Net.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hipsy.Net.Tests;

/// <summary>
/// Calls the real Hipsy API. Skipped unless an API key is configured:
/// <code>
/// dotnet user-secrets set "Hipsy:ApiKey" "your-key" --project tests/Hipsy.Net.Tests
/// dotnet user-secrets set "Hipsy:Organisation" "your-org-slug-or-id" --project tests/Hipsy.Net.Tests   (optional)
/// </code>
/// Environment variables (Hipsy__ApiKey, Hipsy__Organisation) work too. These tests only read data.
/// </summary>
[Trait("Category", "Live")]
public sealed class LiveApiTests : IAsyncDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IHipsyClient _client;
    private readonly string? _organisation;
    private readonly bool _configured;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public LiveApiTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<LiveApiTests>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        _configured = !string.IsNullOrWhiteSpace(configuration["Hipsy:ApiKey"]);
        _organisation = configuration["Hipsy:Organisation"];

        var services = new ServiceCollection();
        services.AddHipsyClient(configuration);
        _provider = services.BuildServiceProvider();
        _client = _provider.GetRequiredService<IHipsyClient>();
    }

    [Fact]
    public async Task Api_key_is_valid()
    {
        SkipIfNotConfigured();

        var user = await _client.GetCurrentUserAsync(Ct);

        Assert.True(user.Id > 0);
        Assert.False(string.IsNullOrEmpty(user.Email));
    }

    [Fact]
    public async Task Lists_organisations()
    {
        SkipIfNotConfigured();

        var organisations = await _client.GetOrganisationsAsync(Ct);

        Assert.NotEmpty(organisations);
        Assert.All(organisations, o => Assert.False(string.IsNullOrEmpty(o.Slug)));
    }

    [Fact]
    public async Task Lists_events_for_each_period()
    {
        var organisation = await GetOrganisationAsync();

        foreach (var period in Enum.GetValues<EventPeriod>())
        {
            var page = await _client.GetEventsAsync(organisation, new EventQuery { Period = period, Limit = 5 }, Ct);
            Assert.True(page.Data.Count <= 5);
            Assert.All(page.Data, e => Assert.True(e.Id > 0));
        }
    }

    [Fact]
    public async Task Gets_single_event_by_id()
    {
        var organisation = await GetOrganisationAsync();
        var first = (await _client.GetEventsAsync(organisation, new EventQuery { Period = EventPeriod.All, Limit = 1 }, Ct)).Data.FirstOrDefault();
        Assert.SkipWhen(first is null, "The organisation has no events.");

        var ev = await _client.GetEventAsync(organisation, first!.Id, cancellationToken: Ct);

        Assert.Equal(first.Id, ev?.Id);
    }

    [Fact]
    public async Task Lists_orders()
    {
        var organisation = await GetOrganisationAsync();

        var page = await _client.GetOrdersAsync(organisation, new OrderQuery { Limit = 5 }, Ct);

        Assert.True(page.Data.Count <= 5);
        Assert.All(page.Data, o => Assert.True(o.Id > 0));
    }

    [Fact]
    public async Task Lists_reviews()
    {
        var organisation = await GetOrganisationAsync();

        var page = await _client.GetReviewsAsync(organisation, 1, Ct);

        Assert.True(page.Data.Count <= 15);
        Assert.All(page.Data, r => Assert.InRange(r.Rating, 1, 5));
    }

    [Fact]
    public void Synchronous_methods_work_against_the_real_api()
    {
        SkipIfNotConfigured();

        var user = _client.GetCurrentUser(Ct);
        var organisations = _client.GetOrganisations(Ct);
        Assert.True(user.Id > 0);
        Assert.SkipWhen(organisations.Count == 0, "The API key has no organisations.");

        OrganisationKey organisation = string.IsNullOrWhiteSpace(_organisation) ? organisations[0].Id : _organisation;
        var events = _client.GetEvents(organisation, new EventQuery { Period = EventPeriod.All, Limit = 1 }, Ct);
        Assert.True(events.Data.Count <= 1);
    }

    private void SkipIfNotConfigured() =>
        Assert.SkipUnless(_configured, "No Hipsy API key configured. Set 'Hipsy:ApiKey' in dotnet user-secrets.");

    private async Task<OrganisationKey> GetOrganisationAsync()
    {
        SkipIfNotConfigured();
        if (!string.IsNullOrWhiteSpace(_organisation))
        {
            return _organisation;
        }

        var organisations = await _client.GetOrganisationsAsync(Ct);
        Assert.SkipWhen(organisations.Count == 0, "The API key has no organisations.");
        return organisations[0].Id;
    }

    public ValueTask DisposeAsync() => _provider.DisposeAsync();
}
