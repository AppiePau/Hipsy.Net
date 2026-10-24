using System.Globalization;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Hipsy.Net.Models;
using Hipsy.Net.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hipsy.Net;

/// <inheritdoc cref="IHipsyClient"/>
public sealed class HipsyClient : IHipsyClient
{
    private const int MaxLimit = 100;

    private readonly HttpClient _httpClient;
    private readonly HipsyClientOptions _options;

    /// <summary>Creates a client. Used by dependency injection, see <see cref="HipsyServiceCollectionExtensions"/>.</summary>
    [ActivatorUtilitiesConstructor]
    public HipsyClient(HttpClient httpClient, IOptions<HipsyClientOptions> options)
        : this(httpClient, options.Value)
    {
    }

    /// <summary>Creates a client without dependency injection.</summary>
    public HipsyClient(HttpClient httpClient, HipsyClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        _httpClient = httpClient;
        _options = options;
    }

    // ---- Current account ----

    /// <inheritdoc />
    public async Task<HipsyUser> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        using var document = await GetJsonAsync(MePath, cancellationToken).ConfigureAwait(false);
        return Deserialize<HipsyUser>(UnwrapData(document.RootElement));
    }

    /// <inheritdoc />
    public HipsyUser GetCurrentUser(CancellationToken cancellationToken = default)
    {
        using var document = GetJson(MePath, cancellationToken);
        return Deserialize<HipsyUser>(UnwrapData(document.RootElement));
    }

    // ---- Organisations ----

    /// <inheritdoc />
    public async Task<IReadOnlyList<Organisation>> GetOrganisationsAsync(CancellationToken cancellationToken = default)
    {
        using var document = await GetJsonAsync(OrganisationsPath, cancellationToken).ConfigureAwait(false);
        return Deserialize<List<Organisation>>(UnwrapData(document.RootElement));
    }

    /// <inheritdoc />
    public IReadOnlyList<Organisation> GetOrganisations(CancellationToken cancellationToken = default)
    {
        using var document = GetJson(OrganisationsPath, cancellationToken);
        return Deserialize<List<Organisation>>(UnwrapData(document.RootElement));
    }

    // ---- Events ----

    /// <inheritdoc />
    public Task<PagedResponse<Event>> GetEventsAsync(OrganisationKey organisation, EventQuery? query = null, CancellationToken cancellationToken = default) =>
        GetPageAsync<Event>(EventsUri(organisation, query ?? new EventQuery()), cancellationToken);

    /// <inheritdoc />
    public PagedResponse<Event> GetEvents(OrganisationKey organisation, EventQuery? query = null, CancellationToken cancellationToken = default) =>
        GetPage<Event>(EventsUri(organisation, query ?? new EventQuery()), cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<Event> GetAllEventsAsync(OrganisationKey organisation, EventQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new EventQuery();
        return EnumerateAsync(page => GetEventsAsync(organisation, query with { Page = page }, cancellationToken), query.Page, cancellationToken);
    }

    /// <inheritdoc />
    public IEnumerable<Event> GetAllEvents(OrganisationKey organisation, EventQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new EventQuery();
        return Enumerate(page => GetEvents(organisation, query with { Page = page }, cancellationToken), query.Page, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Event?> GetEventAsync(OrganisationKey organisation, int eventId, bool includeConcepts = false, CancellationToken cancellationToken = default)
    {
        var page = await GetPageAsync<Event>(SingleEventUri(organisation, eventId, includeConcepts), cancellationToken).ConfigureAwait(false);
        return PickEvent(page, eventId);
    }

    /// <inheritdoc />
    public Event? GetEvent(OrganisationKey organisation, int eventId, bool includeConcepts = false, CancellationToken cancellationToken = default) =>
        PickEvent(GetPage<Event>(SingleEventUri(organisation, eventId, includeConcepts), cancellationToken), eventId);

    // ---- Orders ----

    /// <inheritdoc />
    public Task<PagedResponse<Order>> GetOrdersAsync(OrganisationKey organisation, OrderQuery? query = null, CancellationToken cancellationToken = default) =>
        GetPageAsync<Order>(OrdersUri(organisation, query ?? new OrderQuery()), cancellationToken);

    /// <inheritdoc />
    public PagedResponse<Order> GetOrders(OrganisationKey organisation, OrderQuery? query = null, CancellationToken cancellationToken = default) =>
        GetPage<Order>(OrdersUri(organisation, query ?? new OrderQuery()), cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<Order> GetAllOrdersAsync(OrganisationKey organisation, OrderQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new OrderQuery();
        return EnumerateAsync(page => GetOrdersAsync(organisation, query with { Page = page }, cancellationToken), query.Page, cancellationToken);
    }

    /// <inheritdoc />
    public IEnumerable<Order> GetAllOrders(OrganisationKey organisation, OrderQuery? query = null, CancellationToken cancellationToken = default)
    {
        query ??= new OrderQuery();
        return Enumerate(page => GetOrders(organisation, query with { Page = page }, cancellationToken), query.Page, cancellationToken);
    }

    // ---- Reviews ----

    /// <inheritdoc />
    public Task<PagedResponse<Review>> GetReviewsAsync(OrganisationKey organisation, int page = 1, CancellationToken cancellationToken = default) =>
        GetPageAsync<Review>(ReviewsUri(organisation, page), cancellationToken);

    /// <inheritdoc />
    public PagedResponse<Review> GetReviews(OrganisationKey organisation, int page = 1, CancellationToken cancellationToken = default) =>
        GetPage<Review>(ReviewsUri(organisation, page), cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<Review> GetAllReviewsAsync(OrganisationKey organisation, CancellationToken cancellationToken = default) =>
        EnumerateAsync(page => GetReviewsAsync(organisation, page, cancellationToken), startPage: null, cancellationToken);

    /// <inheritdoc />
    public IEnumerable<Review> GetAllReviews(OrganisationKey organisation, CancellationToken cancellationToken = default) =>
        Enumerate(page => GetReviews(organisation, page, cancellationToken), startPage: null, cancellationToken);

    // ---- Request URIs ----

    private const string MePath = "me";
    private const string OrganisationsPath = "organisations/index";

    private static string EventsUri(OrganisationKey organisation, EventQuery query)
    {
        var parameters = new List<KeyValuePair<string, string>>();
        if (query.Period is { } period)
        {
            parameters.Add(new("period", period.ToString().ToLowerInvariant()));
        }

        if (query.IncludeConcepts)
        {
            parameters.Add(new("concepts", "1"));
        }

        AddPaging(parameters, query.Limit, query.Page);
        return OrganisationPath(organisation, "events") + BuildQueryString(parameters);
    }

    private static string SingleEventUri(OrganisationKey organisation, int eventId, bool includeConcepts)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(eventId);
        var parameters = new List<KeyValuePair<string, string>> { new("id", eventId.ToString(CultureInfo.InvariantCulture)) };
        if (includeConcepts)
        {
            parameters.Add(new("concepts", "1"));
        }

        return OrganisationPath(organisation, "events") + BuildQueryString(parameters);
    }

    private static string OrdersUri(OrganisationKey organisation, OrderQuery query)
    {
        var parameters = new List<KeyValuePair<string, string>>();
        if (query.EventIds is { Count: > 0 } eventIds)
        {
            parameters.Add(new("event", string.Join(',', eventIds.Select(id => id.ToString(CultureInfo.InvariantCulture)))));
        }

        AddPaging(parameters, query.Limit, query.Page);
        return OrganisationPath(organisation, "orders") + BuildQueryString(parameters);
    }

    private static string ReviewsUri(OrganisationKey organisation, int page)
    {
        var parameters = new List<KeyValuePair<string, string>>();
        AddPaging(parameters, limit: null, page);
        return OrganisationPath(organisation, "reviews") + BuildQueryString(parameters);
    }

    private static Event? PickEvent(PagedResponse<Event> page, int eventId) =>
        page.Data.FirstOrDefault(e => e.Id == eventId) ?? page.Data.FirstOrDefault();

    // ---- Paging ----
    // Pages are followed by number instead of the "next" link, because the reviews endpoint returns relative links.

    private static async IAsyncEnumerable<T> EnumerateAsync<T>(
        Func<int, Task<PagedResponse<T>>> getPage,
        int? startPage,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var pageNumber = startPage ?? 1;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var page = await getPage(pageNumber).ConfigureAwait(false);
            foreach (var item in page.Data)
            {
                yield return item;
            }

            if (!page.HasNextPage || page.Data.Count == 0)
            {
                yield break;
            }

            pageNumber = page.Meta.CurrentPage + 1;
        }
    }

    private static IEnumerable<T> Enumerate<T>(Func<int, PagedResponse<T>> getPage, int? startPage, CancellationToken cancellationToken)
    {
        var pageNumber = startPage ?? 1;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var page = getPage(pageNumber);
            foreach (var item in page.Data)
            {
                yield return item;
            }

            if (!page.HasNextPage || page.Data.Count == 0)
            {
                yield break;
            }

            pageNumber = page.Meta.CurrentPage + 1;
        }
    }

    private async Task<PagedResponse<T>> GetPageAsync<T>(string relativeUri, CancellationToken cancellationToken)
    {
        using var document = await GetJsonAsync(relativeUri, cancellationToken).ConfigureAwait(false);
        return Deserialize<PagedResponse<T>>(document.RootElement);
    }

    private PagedResponse<T> GetPage<T>(string relativeUri, CancellationToken cancellationToken)
    {
        using var document = GetJson(relativeUri, cancellationToken);
        return Deserialize<PagedResponse<T>>(document.RootElement);
    }

    // ---- HTTP ----

    private async Task<JsonDocument> GetJsonAsync(string relativeUri, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(relativeUri);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw CreateException(response, body);
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    private JsonDocument GetJson(string relativeUri, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(relativeUri);
        using var response = _httpClient.Send(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        using var stream = response.Content.ReadAsStream(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            using var reader = new StreamReader(stream);
            throw CreateException(response, reader.ReadToEnd());
        }

        return JsonDocument.Parse(stream);
    }

    private HttpRequestMessage CreateRequest(string relativeUri)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "No Hipsy API key is configured. Set HipsyClientOptions.ApiKey (configuration key 'Hipsy:ApiKey').");
        }

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseAddress, relativeUri));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private Uri BaseAddress
    {
        get
        {
            var baseAddress = _options.BaseAddress ?? HipsyClientOptions.DefaultBaseAddress;
            return baseAddress.AbsoluteUri.EndsWith('/') ? baseAddress : new Uri(baseAddress.AbsoluteUri + "/");
        }
    }

    private static HipsyApiException CreateException(HttpResponseMessage response, string body)
    {
        var message = TryReadMessage(body) ?? response.ReasonPhrase ?? "Request failed";

        TimeSpan? retryAfter = response.Headers.RetryAfter switch
        {
            { Delta: { } delta } => delta,
            { Date: { } date } => date - DateTimeOffset.UtcNow,
            _ => null,
        };

        return new HipsyApiException(
            response.StatusCode,
            $"Hipsy API returned {(int)response.StatusCode} ({response.StatusCode}): {message}",
            body,
            retryAfter);
    }

    private static string? TryReadMessage(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String
                    ? message.GetString()
                    : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // ---- JSON ----

    // Some endpoints wrap the result in { "data": ... }, the /me example doesn't. Accept both.
    private static JsonElement UnwrapData(JsonElement root) =>
        root.ValueKind == JsonValueKind.Object
        && root.TryGetProperty("data", out var data)
        && data.ValueKind is JsonValueKind.Object or JsonValueKind.Array
            ? data
            : root;

    private static T Deserialize<T>(JsonElement element) =>
        element.Deserialize<T>(HipsyJson.Options)
        ?? throw new JsonException($"The Hipsy API returned an empty response for {typeof(T).Name}.");

    private static string OrganisationPath(OrganisationKey organisation, string resource)
    {
        if (string.IsNullOrEmpty(organisation.Value))
        {
            throw new ArgumentException("An organisation slug or ID is required.", nameof(organisation));
        }

        return $"organisation/{Uri.EscapeDataString(organisation.Value)}/{resource}";
    }

    private static void AddPaging(List<KeyValuePair<string, string>> parameters, int? limit, int? page)
    {
        if (limit is { } l)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(l, 1, nameof(limit));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(l, MaxLimit, nameof(limit));
            parameters.Add(new("limit", l.ToString(CultureInfo.InvariantCulture)));
        }

        if (page is { } p)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(p, 1, nameof(page));
            parameters.Add(new("page", p.ToString(CultureInfo.InvariantCulture)));
        }
    }

    private static string BuildQueryString(List<KeyValuePair<string, string>> parameters)
    {
        if (parameters.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder("?");
        foreach (var (key, value) in parameters)
        {
            if (builder.Length > 1)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(key)).Append('=').Append(Uri.EscapeDataString(value));
        }

        return builder.ToString();
    }
}
