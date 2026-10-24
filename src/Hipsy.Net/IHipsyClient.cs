using Hipsy.Net.Models;

namespace Hipsy.Net;

/// <summary>
/// Client for the Hipsy API (https://docs.hipsy.nl). All calls throw <see cref="HipsyApiException"/>
/// when the API returns an error status code.
/// </summary>
/// <remarks>
/// Every call has an async version and a synchronous version. The synchronous versions use
/// <see cref="HttpClient.Send(HttpRequestMessage, HttpCompletionOption, CancellationToken)"/> and block the calling
/// thread; prefer the async versions in web apps and UI code. They need every HTTP message handler in the pipeline to
/// support synchronous sending (the default handlers do; Blazor WebAssembly doesn't).
/// </remarks>
public interface IHipsyClient
{
    /// <summary>Gets the user the API key belongs to. Useful to check if a key is valid.</summary>
    Task<HipsyUser> GetCurrentUserAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetCurrentUserAsync"/>
    HipsyUser GetCurrentUser(CancellationToken cancellationToken = default);

    /// <summary>Lists all organisations the user of the API key has access to. Not paginated.</summary>
    Task<IReadOnlyList<Organisation>> GetOrganisationsAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetOrganisationsAsync"/>
    IReadOnlyList<Organisation> GetOrganisations(CancellationToken cancellationToken = default);

    /// <summary>Gets one page of events of an organisation.</summary>
    Task<PagedResponse<Event>> GetEventsAsync(OrganisationKey organisation, EventQuery? query = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetEventsAsync"/>
    PagedResponse<Event> GetEvents(OrganisationKey organisation, EventQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets all events of an organisation, requesting pages as you enumerate.</summary>
    IAsyncEnumerable<Event> GetAllEventsAsync(OrganisationKey organisation, EventQuery? query = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetAllEventsAsync"/>
    IEnumerable<Event> GetAllEvents(OrganisationKey organisation, EventQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a single event by ID, or null if the organisation has no event with that ID.</summary>
    Task<Event?> GetEventAsync(OrganisationKey organisation, int eventId, bool includeConcepts = false, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetEventAsync"/>
    Event? GetEvent(OrganisationKey organisation, int eventId, bool includeConcepts = false, CancellationToken cancellationToken = default);

    /// <summary>Gets one page of paid orders of an organisation, newest first.</summary>
    Task<PagedResponse<Order>> GetOrdersAsync(OrganisationKey organisation, OrderQuery? query = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetOrdersAsync"/>
    PagedResponse<Order> GetOrders(OrganisationKey organisation, OrderQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets all paid orders of an organisation, requesting pages as you enumerate.</summary>
    IAsyncEnumerable<Order> GetAllOrdersAsync(OrganisationKey organisation, OrderQuery? query = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetAllOrdersAsync"/>
    IEnumerable<Order> GetAllOrders(OrganisationKey organisation, OrderQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one page of reviews (always 15 per page), newest first.</summary>
    Task<PagedResponse<Review>> GetReviewsAsync(OrganisationKey organisation, int page = 1, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetReviewsAsync"/>
    PagedResponse<Review> GetReviews(OrganisationKey organisation, int page = 1, CancellationToken cancellationToken = default);

    /// <summary>Gets all reviews of an organisation, requesting pages as you enumerate.</summary>
    IAsyncEnumerable<Review> GetAllReviewsAsync(OrganisationKey organisation, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetAllReviewsAsync"/>
    IEnumerable<Review> GetAllReviews(OrganisationKey organisation, CancellationToken cancellationToken = default);
}
