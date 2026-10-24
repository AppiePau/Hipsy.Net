namespace Hipsy.Net.Models;

/// <summary>The user that the API key belongs to (<c>GET /me</c>).</summary>
public sealed record HipsyUser
{
    /// <summary>The ID of the user.</summary>
    public int Id { get; init; }

    /// <summary>The full name of the user.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The email address of the user.</summary>
    public string Email { get; init; } = string.Empty;
}

/// <summary>An organisation the user of the API key has access to.</summary>
public sealed record Organisation
{
    /// <summary>The ID of the organisation. Prefer this over the slug in URLs.</summary>
    public int Id { get; init; }

    /// <summary>The name of the organisation.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The URL slug of the organisation.</summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>URL of the logo of the organisation.</summary>
    public string? Logo { get; init; }
}
