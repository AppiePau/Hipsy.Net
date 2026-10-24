using System.Net;
using System.Text;

namespace Hipsy.Net.Tests;

/// <summary>Returns queued responses and records the requests it received.</summary>
internal sealed class StubHttpHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

    public List<HttpRequestMessage> Requests { get; } = [];

    public StubHttpHandler RespondJson(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        _responses.Enqueue(_ => new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        return this;
    }

    public StubHttpHandler Respond(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        _responses.Enqueue(respond);
        return this;
    }

    public int SyncSends { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(Next(request));

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SyncSends++;
        return Next(request);
    }

    private HttpResponseMessage Next(HttpRequestMessage request)
    {
        Requests.Add(request);
        if (_responses.Count == 0)
        {
            throw new InvalidOperationException($"Unexpected request: {request.RequestUri}");
        }

        return _responses.Dequeue()(request);
    }

    public static (HipsyClient Client, StubHttpHandler Handler) CreateClient(string apiKey = "test-key")
    {
        var handler = new StubHttpHandler();
        var client = new HipsyClient(new HttpClient(handler), new HipsyClientOptions { ApiKey = apiKey });
        return (client, handler);
    }
}
