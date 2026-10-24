namespace Hipsy.Net.Models;

/// <summary>A review a visitor wrote about an event.</summary>
public sealed record Review
{
    /// <summary>The ID of the review.</summary>
    public int Id { get; init; }

    /// <summary>The writer of the review.</summary>
    public Reviewer Reviewer { get; init; } = new();

    /// <summary>The name of the event the review is about.</summary>
    public string Event { get; init; } = string.Empty;

    /// <summary>The text of the review.</summary>
    public string? Text { get; init; }

    /// <summary>The rating from 1 to 5.</summary>
    public int Rating { get; init; }

    /// <summary>The reply of the organisation, or null.</summary>
    public string? Reply { get; init; }

    /// <summary>When the organisation replied, or null.</summary>
    public DateTimeOffset? RepliedAt { get; init; }

    /// <summary>When the review was written.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>The writer of a review.</summary>
public sealed record Reviewer
{
    /// <summary>The ID of the user who wrote the review.</summary>
    public int Id { get; init; }

    /// <summary>The name the reviewer chose to show, or "Anonymous" in their language.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Always null. Reserved for future use.</summary>
    public string? AvatarUrl { get; init; }
}
