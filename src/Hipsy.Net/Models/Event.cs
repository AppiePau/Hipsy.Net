namespace Hipsy.Net.Models;

/// <summary>An event of an organisation.</summary>
public sealed record Event
{
    /// <summary>The ID of the event.</summary>
    public int Id { get; init; }

    /// <summary>The title of the event.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The description of the event.</summary>
    public string? Description { get; init; }

    /// <summary>The location of the event.</summary>
    public string? Location { get; init; }

    /// <summary>The latitude of the location.</summary>
    public double? Latitude { get; init; }

    /// <summary>The longitude of the location.</summary>
    public double? Longitude { get; init; }

    /// <summary>URL of the picture. Empty if the event has no picture.</summary>
    public string? Picture { get; init; }

    /// <summary>URL of the picture. Currently the same as <see cref="Picture"/>.</summary>
    public string? PictureSmall { get; init; }

    /// <summary>When the event starts (UTC).</summary>
    public DateTimeOffset Date { get; init; }

    /// <summary>When the event ends (UTC).</summary>
    public DateTimeOffset? DateUntil { get; init; }

    /// <summary>True if the event is sold out.</summary>
    public bool SoldOut { get; init; }

    /// <summary>True if the event is only accessible with a direct link.</summary>
    public bool Private { get; init; }

    /// <summary>True if ticket sales via Hipsy are disabled for this event.</summary>
    public bool DisableTickets { get; init; }

    /// <summary>URL of the event page on the Hipsy website.</summary>
    public string? UrlHipsy { get; init; }

    /// <summary>URL of the ticketshop of the event.</summary>
    public string? UrlTicketshop { get; init; }

    /// <summary>URL of a QR image that links to the ticketshop.</summary>
    public string? QrTicketshop { get; init; }

    /// <summary>The configured tickets of the event.</summary>
    public IReadOnlyList<EventTicket> Tickets { get; init; } = [];

    /// <summary>The names of the categories of the event.</summary>
    public IReadOnlyList<string> Categories { get; init; } = [];
}

/// <summary>A ticket type configured for an event.</summary>
public sealed record EventTicket
{
    /// <summary>The value Hipsy uses for <see cref="Available"/> when the amount is unlimited.</summary>
    public const int UnlimitedAvailability = 99999;

    /// <summary>The ID of the configured ticket.</summary>
    public int Id { get; init; }

    /// <summary>Ticket, donation or combiticket.</summary>
    public TicketType Type { get; init; }

    /// <summary>The name of the ticket.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The description of the ticket.</summary>
    public string? Description { get; init; }

    /// <summary>The price of the ticket.</summary>
    public decimal Price { get; init; }

    /// <summary>True if the ticket gives access to the event.</summary>
    public bool IsEntryTicket { get; init; }

    /// <summary>The sale status of the ticket.</summary>
    public TicketStatus Status { get; init; }

    /// <summary>The amount available to buy. 99999 means unlimited, see <see cref="IsUnlimited"/>.</summary>
    public int Available { get; init; }

    /// <summary>When the sale starts, if set.</summary>
    public DateTimeOffset? SaleStart { get; init; }

    /// <summary>When the sale ends, if set.</summary>
    public DateTimeOffset? SaleEnd { get; init; }

    /// <summary>Only for invisible tickets: the direct URL to buy this ticket.</summary>
    public string? Url { get; init; }

    /// <summary>True if there is no limit on the amount of tickets.</summary>
    public bool IsUnlimited => Available >= UnlimitedAvailability;
}

/// <summary>The kind of a configured ticket.</summary>
public enum TicketType
{
    /// <summary>A value this client version doesn't know.</summary>
    Unknown = 0,
    /// <summary>A regular ticket.</summary>
    Ticket,
    /// <summary>A donation.</summary>
    Donation,
    /// <summary>A combination ticket.</summary>
    Combiticket,
}

/// <summary>The sale status of a configured ticket.</summary>
public enum TicketStatus
{
    /// <summary>A value this client version doesn't know.</summary>
    Unknown = 0,
    /// <summary>On sale.</summary>
    Sale,
    /// <summary>Sold out.</summary>
    Sold,
    /// <summary>Hidden; only buyable with its direct URL.</summary>
    Invisible,
    /// <summary>Not available.</summary>
    NotAvailable,
    /// <summary>The sale hasn't started yet.</summary>
    StartFuture,
}

/// <summary>Which events to return.</summary>
public enum EventPeriod
{
    /// <summary>Upcoming events, soonest first (the API default).</summary>
    Upcoming,
    /// <summary>Past events, most recent first.</summary>
    Past,
    /// <summary>All events, most recent first.</summary>
    All,
}
