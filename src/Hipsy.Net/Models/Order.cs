using System.Text.Json.Serialization;

namespace Hipsy.Net.Models;

/// <summary>A paid ticket order.</summary>
public sealed record Order
{
    /// <summary>The ID of the order.</summary>
    public int Id { get; init; }

    /// <summary>The ID of the event.</summary>
    public int EventId { get; init; }

    /// <summary>The name of the event.</summary>
    public string EventName { get; init; } = string.Empty;

    /// <summary>The revenue paid out to the organisation, excluding service fees.</summary>
    public decimal Revenue { get; init; }

    /// <summary>First name of the buyer.</summary>
    public string? FirstName { get; init; }

    /// <summary>Last name of the buyer.</summary>
    public string? LastName { get; init; }

    /// <summary>Email address of the buyer.</summary>
    public string? Email { get; init; }

    /// <summary>Language the buyer used: nl, en, de, fr or es.</summary>
    public string? Locale { get; init; }

    /// <summary>True if the buyer subscribed to the newsletter.</summary>
    public bool Newsletter { get; init; }

    /// <summary>When the order was placed (UTC).</summary>
    public DateTimeOffset Date { get; init; }

    /// <summary>The answers of the buyer to the order form.</summary>
    public OrderPersonalData PersonalData { get; init; } = new();

    /// <summary>UTM parameters of the visit that led to the order.</summary>
    public OrderUtm Utm { get; init; } = new();

    /// <summary>The tickets in the order, including cancelled tickets.</summary>
    public IReadOnlyList<OrderTicket> Tickets { get; init; } = [];
}

/// <summary>Answers to the order form. A value is null if the field isn't on the form or wasn't filled in.</summary>
public sealed record OrderPersonalData
{
    /// <summary>City.</summary>
    public string? City { get; init; }
    /// <summary>Phone number.</summary>
    public string? Phone { get; init; }
    /// <summary>Gender.</summary>
    public string? Gender { get; init; }
    /// <summary>Address.</summary>
    public string? Address { get; init; }
    /// <summary>Comment.</summary>
    public string? Comment { get; init; }
    /// <summary>Company.</summary>
    public string? Company { get; init; }
    /// <summary>Dietary wishes.</summary>
    public string? Dietary { get; init; }
    /// <summary>Birthday, as entered in the form.</summary>
    public string? Birthday { get; init; }
    /// <summary>Postcode.</summary>
    public string? Postcode { get; init; }
    /// <summary>Answer to custom question 1.</summary>
    [JsonPropertyName("question1")] public string? Question1 { get; init; }
    /// <summary>Answer to custom question 2.</summary>
    [JsonPropertyName("question2")] public string? Question2 { get; init; }
    /// <summary>Answer to custom question 3.</summary>
    [JsonPropertyName("question3")] public string? Question3 { get; init; }
}

/// <summary>UTM parameters. A value is null if it wasn't provided.</summary>
public sealed record OrderUtm
{
    /// <summary>utm_source.</summary>
    public string? UtmSource { get; init; }
    /// <summary>utm_medium.</summary>
    public string? UtmMedium { get; init; }
    /// <summary>utm_campaign.</summary>
    public string? UtmCampaign { get; init; }
    /// <summary>utm_term.</summary>
    public string? UtmTerm { get; init; }
    /// <summary>utm_content.</summary>
    public string? UtmContent { get; init; }
}

/// <summary>A ticket in an order.</summary>
public sealed record OrderTicket
{
    /// <summary>The ticket number.</summary>
    public long Number { get; init; }

    /// <summary>The ID of the configured ticket (see <see cref="EventTicket.Id"/>).</summary>
    public int EventTicketId { get; init; }

    /// <summary>The name of the configured ticket.</summary>
    public string EventTicketName { get; init; } = string.Empty;

    /// <summary>True if the ticket has been checked in at the event.</summary>
    public bool Checkin { get; init; }

    /// <summary>The price of the ticket. The API sends this as a string such as "17.50".</summary>
    public decimal PriceTicket { get; init; }

    /// <summary>The price including service fee. The API sends this as a string such as "18.45".</summary>
    public decimal PriceFinal { get; init; }
}
