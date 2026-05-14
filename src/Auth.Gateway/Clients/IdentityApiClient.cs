using Microsoft.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Auth.Gateway.Clients;

public class IdentityApiClient
{
    private readonly HttpClient _client;
    public const string RequestIdHeaderName = "Request-ID";

    public IdentityApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<IResult> ForwardJsonAsync<T>(HttpContext context, HttpMethod method, string upstreamPath, T body, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await ForwardAsync(context, method, upstreamPath, content, ct);
    }

    public async Task<IResult> ForwardAsync(HttpContext context, HttpMethod method, string upstreamPath, HttpContent? content, CancellationToken ct)
    {
        using var upstreamRequest = new HttpRequestMessage(method, upstreamPath)
        {
            Content = content
        };

        CopyRequestHeaders(context, upstreamRequest);

        using var upstreamResponse = await _client.SendAsync(
            upstreamRequest,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        CopyResponseHeaders(context, upstreamResponse);

        var statusCode = (int)upstreamResponse.StatusCode;

        string? responseBody = upstreamResponse.Content is null
            ? null
            : await upstreamResponse.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return Results.StatusCode(statusCode);
        }

        var contentType = upstreamResponse.Content?.Headers.ContentType?.ToString();

        return Results.Text(
            responseBody,
            contentType: contentType ?? "text/plain; charset=utf-8",
            statusCode: statusCode);
    }

    private void CopyRequestHeaders(HttpContext context, HttpRequestMessage upstreamRequest)
    {
        var headers = context.Request.Headers;

        if (headers.TryGetValue(HeaderNames.Authorization, out var authorization))
        {
            _ = upstreamRequest.Headers.TryAddWithoutValidation(HeaderNames.Authorization, authorization.ToString());
        }

        if (headers.TryGetValue(HeaderNames.Cookie, out var cookie))
        {
            _ = upstreamRequest.Headers.TryAddWithoutValidation(HeaderNames.Cookie, cookie.ToString());
        }

        if (headers.TryGetValue(RequestIdHeaderName, out var requestId))
        {
            _ = upstreamRequest.Headers.TryAddWithoutValidation(RequestIdHeaderName, requestId.ToString());
        }
    }

    private void CopyResponseHeaders(HttpContext context, HttpResponseMessage upstreamResponse)
    {
        if (upstreamResponse.Headers.TryGetValues(HeaderNames.SetCookie, out var setCookies))
        {
            foreach (var setCookie in setCookies)
            {
                context.Response.Headers.Append(HeaderNames.SetCookie, setCookie);
            }
        }

        if (upstreamResponse.Headers.TryGetValues(HeaderNames.WWWAuthenticate, out var challenges))
        {
            foreach (var challenge in challenges)
            {
                context.Response.Headers.Append(HeaderNames.WWWAuthenticate, challenge);
            }
        }
    }
}
