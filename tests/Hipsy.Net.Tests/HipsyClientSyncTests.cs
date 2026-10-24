using System.Net;
using Hipsy.Net.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Hipsy.Net.Tests;

/// <summary>The synchronous methods share URL building and parsing with the async ones; these check the sync path itself.</summary>
public class HipsyClientSyncTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void Gets_current_user_with_a_synchronous_send()
    {
        var (client, handler) = StubHttpHandler.CreateClient("secret-key");
        handler.RespondJson(DocExamples.Me);

        var user = client.GetCurrentUser(Ct);

        Assert.Equal(1234, user.Id);
        Assert.Equal(1, handler.SyncSends);
        Assert.Equal("secret-key", handler.Requests[0].Headers.Authorization!.Parameter);
    }

    [Fact]
    public void Gets_organisations()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.Organisations);

        Assert.Equal("my-organisation", Assert.Single(client.GetOrganisations(Ct)).Slug);
    }

    [Fact]
    public void Gets_events_with_query()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.Events);

        var page = client.GetEvents(1000, new EventQuery { Period = EventPeriod.All, Limit = 10 }, Ct);

        Assert.Equal("https://api.hipsy.nl/v1/organisation/1000/events?period=all&limit=10", handler.Requests[0].RequestUri!.ToString());
        Assert.Equal(TicketStatus.StartFuture, page.Data[0].Tickets[1].Status);
    }

    [Fact]
    public void Gets_single_event()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.Events);

        Assert.Equal(1032, client.GetEvent("my-org", 1032, cancellationToken: Ct)?.Id);
    }

    [Fact]
    public void Gets_orders_with_string_prices()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.Orders);

        var ticket = client.GetOrders("my-org", cancellationToken: Ct).Data[0].Tickets[0];

        Assert.Equal(18.45m, ticket.PriceFinal);
    }

    [Fact]
    public void Gets_reviews()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.Reviews);

        Assert.Equal(5, client.GetReviews("my-org", cancellationToken: Ct).Data[0].Rating);
    }

    [Fact]
    public void Get_all_follows_pages_lazily()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.ReviewPage(1, 3))
               .RespondJson(DocExamples.ReviewPage(2, 3))
               .RespondJson(DocExamples.ReviewPage(3, 3));

        var reviews = client.GetAllReviews("my-org", Ct);
        Assert.Empty(handler.Requests); // nothing is requested until you enumerate

        Assert.Equal([1, 2, 3], reviews.Select(r => r.Id));
        Assert.Equal(3, handler.SyncSends);
    }

    [Fact]
    public void Throws_api_exception_with_message()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson("""{ "message": "Unauthenticated." }""", HttpStatusCode.Unauthorized);

        var ex = Assert.Throws<HipsyApiException>(() => client.GetOrganisations(Ct));

        Assert.True(ex.IsUnauthorized);
        Assert.Contains("Unauthenticated.", ex.Message);
        Assert.Equal("""{ "message": "Unauthenticated." }""", ex.ResponseBody);
    }

    [Fact]
    public void Works_through_the_http_client_factory_pipeline()
    {
        var handler = new StubHttpHandler().RespondJson(DocExamples.Me);
        var services = new ServiceCollection();
        services.AddHipsyClient(o => o.ApiKey = "di-key")
                .ConfigurePrimaryHttpMessageHandler(() => handler);

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IHipsyClient>().GetCurrentUser(Ct);

        Assert.Equal(1, handler.SyncSends);
    }
}
