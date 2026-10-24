# Hipsy.Net

Unofficial, typed .NET 8 client for the hipsy.nl / hipsy.eu [Hipsy API](https://docs.hipsy.nl/api-reference/getting-started).

> **Not affiliated with Hipsy.** This is an independent open-source project. It is not made, endorsed or
> supported by Hipsy or the operators of [hipsy.nl](https://hipsy.nl) and [hipsy.eu](https://hipsy.eu).
> "Hipsy" is used only to describe which API this library talks to; the name, logos and trademarks belong
> to their owners.

| Method | Endpoint |
|---|---|
| `GetCurrentUserAsync` | `GET /v1/me` |
| `GetOrganisationsAsync` | `GET /v1/organisations/index` |
| `GetEventsAsync` / `GetAllEventsAsync` / `GetEventAsync` | `GET /v1/organisation/{organisation}/events` |
| `GetOrdersAsync` / `GetAllOrdersAsync` | `GET /v1/organisation/{organisation}/orders` |
| `GetReviewsAsync` / `GetAllReviewsAsync` | `GET /v1/organisation/{organisation}/reviews` |

`Get…Async` returns one page (`PagedResponse<T>` with `Data`, `Links`, `Meta`). `GetAll…Async` returns an
`IAsyncEnumerable<T>` that requests the next page as you enumerate. `{organisation}` accepts the slug or the
numeric ID; prefer the ID, because slug URLs break when the slug changes.

## Usage

```csharp
// appsettings.json / user-secrets: { "Hipsy": { "ApiKey": "..." } }
builder.Services.AddHipsyClient(builder.Configuration);

// or in code
builder.Services.AddHipsyClient(o => o.ApiKey = "...");
```

```csharp
public class EventsService(IHipsyClient hipsy)
{
    public async Task PrintAsync(CancellationToken ct)
    {
        var organisation = (await hipsy.GetOrganisationsAsync(ct))[0];

        await foreach (var ev in hipsy.GetAllEventsAsync(organisation.Id, new EventQuery { Period = EventPeriod.Upcoming, Limit = 100 }, ct))
        {
            Console.WriteLine($"{ev.Date:g} {ev.Title} ({ev.Tickets.Count} tickets)");
        }
    }
}
```

### Synchronous methods

Every method also has a synchronous version without the `Async` suffix (`GetCurrentUser`, `GetOrganisations`,
`GetEvents`, `GetAllEvents`, `GetEvent`, `GetOrders`, `GetAllOrders`, `GetReviews`, `GetAllReviews`).
They use `HttpClient.Send`, so they don't wrap the async code. `GetAll…` returns an `IEnumerable<T>` that requests
pages as you enumerate.

```csharp
foreach (var order in hipsy.GetAllOrders(1000, new OrderQuery { Limit = 100 }))
{
    Console.WriteLine($"{order.Date:d} {order.EventName} {order.Revenue:C}");
}
```

They block the calling thread, so prefer the async versions in web apps and UI code. Every HTTP handler you add to
the client must support synchronous sending; the default ones do, Blazor WebAssembly doesn't.

Without dependency injection:

```csharp
var hipsy = new HipsyClient(new HttpClient(), new HipsyClientOptions { ApiKey = "..." });
```

### Errors and rate limits

Error responses throw `HipsyApiException` with `StatusCode`, the API's message, `ResponseBody` and
helpers `IsUnauthorized` (401), `IsForbidden` (403), `IsNotFound` (404) and `IsRateLimited` (429, with `RetryAfter`).
The API allows 120 requests per minute per user. `AddHipsyClient` returns an `IHttpClientBuilder`, so you can add
retries yourself, for example with `Microsoft.Extensions.Http.Resilience`:

```csharp
builder.Services.AddHipsyClient(builder.Configuration).AddStandardResilienceHandler();
```

## Tests

```bash
dotnet test
```

Unit tests replay the example responses from the Hipsy docs. The live tests in `LiveApiTests` call the real API
(read-only) and are skipped until you set a key:

```bash
dotnet user-secrets set "Hipsy:ApiKey" "your-api-key" --project tests/Hipsy.Net.Tests
# optional, otherwise the first organisation of the key is used
dotnet user-secrets set "Hipsy:Organisation" "your-org-id-or-slug" --project tests/Hipsy.Net.Tests
```

Run only the live tests: `dotnet test -- --filter-trait "Category=Live"`.

`global.json` switches `dotnet test` to Microsoft.Testing.Platform, which xunit v3 needs on the .NET 10 SDK.

## License and disclaimer

Released under the [MIT License](LICENSE). Copyright (c) 2026 Paul Appeldoorn.

The software is provided "as is", without warranty of any kind. The authors are not liable for any claim, damages
or other liability arising from its use (see the license for the full terms). In particular:

- **Your data, your responsibility.** The orders endpoint returns personal data of ticket buyers (names, email
  addresses, form answers). You are the one processing that data and are responsible for handling it lawfully,
  for example under the GDPR (AVG).
- **Your API key, your responsibility.** A Hipsy API key gives access to every organisation of its user. Keep it
  out of source control and logs; use user-secrets, environment variables or a secret store.
- **The Hipsy API can change.** This library follows the public documentation at the time of writing. Hipsy can
  change or remove the API at any time, and Hipsy's own terms and conditions (on hipsy.nl and hipsy.eu) apply to
  your use of it.
