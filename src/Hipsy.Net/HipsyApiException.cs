using System.Net;

namespace Hipsy.Net;

/// <summary>
/// Thrown when the Hipsy API returns an unsuccessful status code.
/// </summary>
public class HipsyApiException : Exception
{
    /// <summary>Creates a new exception.</summary>
    public HipsyApiException(HttpStatusCode statusCode, string message, string? responseBody, TimeSpan? retryAfter = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        RetryAfter = retryAfter;
    }

    /// <summary>The HTTP status code returned by the API.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>The raw response body, if any.</summary>
    public string? ResponseBody { get; }

    /// <summary>For a 429 response: how long to wait before the next request, if the API said so.</summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>401: the API key is missing or invalid.</summary>
    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;

    /// <summary>403: the user of the API key has no access to the organisation.</summary>
    public bool IsForbidden => StatusCode == HttpStatusCode.Forbidden;

    /// <summary>404: the organisation (or resource) doesn't exist.</summary>
    public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;

    /// <summary>429: more than 120 requests per minute were sent.</summary>
    public bool IsRateLimited => StatusCode == HttpStatusCode.TooManyRequests;
}
