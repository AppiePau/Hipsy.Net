using System.Globalization;

namespace Hipsy.Net;

/// <summary>
/// Identifies an organisation by its numeric ID or its slug.
/// Converts implicitly from <see cref="int"/> and <see cref="string"/>, so you can pass either.
/// </summary>
/// <remarks>
/// Prefer the ID: endpoints that use a slug stop working when the slug is changed in the dashboard.
/// </remarks>
public readonly record struct OrganisationKey
{
    /// <summary>The value used in the URL path.</summary>
    public string Value { get; }

    /// <summary>Creates a key from a slug or a numeric ID as text.</summary>
    public OrganisationKey(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>Creates a key from a numeric organisation ID.</summary>
    public OrganisationKey(int id)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        Value = id.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>Creates a key from a slug.</summary>
    public static implicit operator OrganisationKey(string slugOrId) => new(slugOrId);

    /// <summary>Creates a key from an ID.</summary>
    public static implicit operator OrganisationKey(int id) => new(id);

    /// <inheritdoc />
    public override string ToString() => Value;
}
