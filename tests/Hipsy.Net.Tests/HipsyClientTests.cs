using System.Net;
using Hipsy.Net.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Hipsy.Net.Tests;

public class HipsyClientTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Sends_bearer_token_and_accept_header()
    {
        var (client, handler) = StubHttpHandler.CreateClient("secret-key");
        handler.RespondJson(DocExamples.Me);

        await client.GetCurrentUserAsync(Ct);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.hipsy.nl/v1/me", request.RequestUri!.ToString());
        Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
        Assert.Equal("secret-key", request.Headers.Authorization.Parameter);
        Assert.Contains(request.Headers.Accept, h => h.MediaType == "application/json");
    }

    [Fact]
    public async Task Reads_current_user_with_or_without_data_wrapper()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.Me).RespondJson($$"""{ "data": {{DocExamples.Me}} }""");

        var bare = await client.GetCurrentUserAsync(Ct);
        var wrapped = await client.GetCurrentUserAsync(Ct);

        Assert.Equal(new HipsyUser { Id = 1234, Name = "Jane Doe", Email = "jane@example.com" }, bare);
        Assert.Equal(bare, wrapped);
    }

    [Fact]
    public async Task Reads_organisations()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.Organisations);

        var organisations = await client.GetOrganisationsAsync(Ct);

        Assert.Equal("https://api.hipsy.nl/v1/organisations/index", handler.Requests[0].RequestUri!.ToString());
        var organisation = Assert.Single(organisations);
        Assert.Equal(1000, organisation.Id);
        Assert.Equal("my-organisation", organisation.Slug);
    }

    [Fact]
    public async Task Reads_events_and_tickets()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.Events);

        var page = await client.GetEventsAsync("ecstatic-dance-universe", cancellationToken: Ct);

        var ev = Assert.Single(page.Data);
        Assert.Equal(1032, ev.Id);
        Assert.Equal(52.1561113, ev.Latitude);
        Assert.Equal(new DateTimeOffset(2023, 3, 1, 19, 0, 0, TimeSpan.Zero), ev.Date);
        Assert.Equal("https://hipsy.nl/shop/1032-ecstatic-dance-universe-kareem", ev.UrlTicketshop);
        Assert.Equal(["Ecstatic Dance"], ev.Categories);

        var earlyBird = ev.Tickets[0];
        Assert.Equal(TicketType.Ticket, earlyBird.Type);
        Assert.Equal(TicketStatus.Sale, earlyBird.Status);
        Assert.Equal(17.5m, earlyBird.Price);
        Assert.True(earlyBird.IsEntryTicket);
        Assert.False(earlyBird.IsUnlimited);
        Assert.Null(earlyBird.SaleEnd);

        var donation = ev.Tickets[1];
        Assert.Equal(TicketType.Donation, donation.Type);
        Assert.Equal(TicketStatus.StartFuture, donation.Status);
        Assert.True(donation.IsUnlimited);

        var unknown = ev.Tickets[2];
        Assert.Equal(TicketType.Unknown, unknown.Type);
        Assert.Equal(TicketStatus.Unknown, unknown.Status);
        Assert.Equal("https://hipsy.nl/shop/secret", unknown.Url);

        Assert.False(page.HasNextPage);
        Assert.Equal(25, page.Meta.PerPage);
    }

    [Fact]
    public async Task Builds_event_query_string()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.Events);

        await client.GetEventsAsync(1000, new EventQuery { Period = EventPeriod.Past, IncludeConcepts = true, Limit = 100, Page = 2 }, Ct);

        Assert.Equal(
            "https://api.hipsy.nl/v1/organisation/1000/events?period=past&concepts=1&limit=100&page=2",
            handler.Requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task Gets_single_event_by_id()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.Events);

        var ev = await client.GetEventAsync("my-org", 1032, cancellationToken: Ct);

        Assert.Equal(1032, ev?.Id);
        Assert.Equal("https://api.hipsy.nl/v1/organisation/my-org/events?id=1032", handler.Requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task Reads_orders_with_string_prices()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.Orders);

        var page = await client.GetOrdersAsync("my-org", new OrderQuery { EventIds = [1030, 1032], Limit = 50 }, Ct);

        Assert.Equal(
            "https://api.hipsy.nl/v1/organisation/my-org/orders?event=1030%2C1032&limit=50",
            handler.Requests[0].RequestUri!.AbsoluteUri);

        var order = Assert.Single(page.Data);
        Assert.Equal(17.5m, order.Revenue);
        Assert.Equal("Utrecht", order.PersonalData.City);
        Assert.Equal("Yes", order.PersonalData.Question1);
        Assert.Equal("newsletter", order.Utm.UtmSource);

        var ticket = Assert.Single(order.Tickets);
        Assert.Equal(114857291, ticket.Number);
        Assert.Equal(17.50m, ticket.PriceTicket);
        Assert.Equal(18.45m, ticket.PriceFinal);

        Assert.True(page.HasNextPage);
        Assert.Equal(81, page.Meta.Total);
    }

    [Fact]
    public async Task Reads_reviews()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.Reviews);

        var page = await client.GetReviewsAsync("my-org", 1, Ct);

        var review = Assert.Single(page.Data);
        Assert.Equal(5, review.Rating);
        Assert.Equal("Jane", review.Reviewer.Name);
        Assert.NotNull(review.RepliedAt);
        Assert.Equal("?page=2", page.Links.Next);
    }

    [Fact]
    public async Task Get_all_follows_pages_until_the_last_one()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.ReviewPage(1, 3))
               .RespondJson(DocExamples.ReviewPage(2, 3))
               .RespondJson(DocExamples.ReviewPage(3, 3));

        var ids = new List<int>();
        await foreach (var review in client.GetAllReviewsAsync("my-org", Ct))
        {
            ids.Add(review.Id);
        }

        Assert.Equal([1, 2, 3], ids);
        Assert.Equal(
            ["page=1", "page=2", "page=3"],
            handler.Requests.Select(r => r.RequestUri!.Query.TrimStart('?')));
    }

    [Fact]
    public async Task Escapes_organisation_slug()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson(DocExamples.Reviews);

        await client.GetReviewsAsync("a b/c", cancellationToken: Ct);

        Assert.Equal("/v1/organisation/a%20b%2Fc/reviews", handler.Requests[0].RequestUri!.AbsolutePath);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task Throws_api_exception_with_message(HttpStatusCode status)
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.RespondJson("""{ "message": "Nope." }""", status);

        var ex = await Assert.ThrowsAsync<HipsyApiException>(() => client.GetEventsAsync("my-org", cancellationToken: Ct));

        Assert.Equal(status, ex.StatusCode);
        Assert.Contains("Nope.", ex.Message);
        Assert.Equal("""{ "message": "Nope." }""", ex.ResponseBody);
    }

    [Fact]
    public async Task Rate_limit_exception_contains_retry_after()
    {
        var (client, handler) = StubHttpHandler.CreateClient();
        handler.Respond(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StringContent("""{"message":"Too Many Attempts."}""") };
            response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(42));
            return response;
        });

        var ex = await Assert.ThrowsAsync<HipsyApiException>(() => client.GetOrganisationsAsync(Ct));

        Assert.True(ex.IsRateLimited);
        Assert.Equal(TimeSpan.FromSeconds(42), ex.RetryAfter);
    }

    [Fact]
    public async Task Missing_api_key_fails_before_sending()
    {
        var (client, handler) = StubHttpHandler.CreateClient(apiKey: "");

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetCurrentUserAsync(Ct));
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Rejects_limit_outside_1_to_100(int limit)
    {
        var (client, _) = StubHttpHandler.CreateClient();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetOrdersAsync("my-org", new OrderQuery { Limit = limit }, Ct));
    }

    [Fact]
    public async Task Can_be_resolved_from_dependency_injection()
    {
        var handler = new StubHttpHandler().RespondJson(DocExamples.Me);
        var services = new ServiceCollection();
        services.AddHipsyClient(o => o.ApiKey = "di-key")
                .ConfigurePrimaryHttpMessageHandler(() => handler);

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHipsyClient>();
        await client.GetCurrentUserAsync(Ct);

        Assert.Equal("di-key", handler.Requests[0].Headers.Authorization!.Parameter);
    }
}
