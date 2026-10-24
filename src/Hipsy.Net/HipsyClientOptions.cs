namespace Hipsy.Net;

/// <summary>
/// Settings for <see cref="HipsyClient"/>. Usually bound from the <c>Hipsy</c> configuration section.
/// </summary>
public sealed class HipsyClientOptions
{
    /// <summary>The default configuration section name.</summary>
    public const string SectionName = "Hipsy";

    /// <summary>The default base address of the Hipsy API.</summary>
    public static readonly Uri DefaultBaseAddress = new("https://api.hipsy.nl/v1/");

    /// <summary>
    /// The API key, created in the Hipsy dashboard under Organisatie &gt; Koppelingen.
    /// The key is bound to a user and gives access to all organisations of that user.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>The base address of the API. Defaults to <c>https://api.hipsy.nl/v1/</c>.</summary>
    public Uri BaseAddress { get; set; } = DefaultBaseAddress;
}
