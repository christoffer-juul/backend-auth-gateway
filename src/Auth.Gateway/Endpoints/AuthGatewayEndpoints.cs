using Auth.Gateway.Dtos;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.Json;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Auth.Gateway.Endpoints;

public static class AuthGatewayEndpoints
{
    private const string IdentityHttpClientName = "IdentityApi";
    private const string EmailVerificationHttpClientName = "EmailVerificationApi";
    private const string RequestIdHeaderName = "Request-ID";

    public static void MapAuthGatewayEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithDescription(
                "Authentication gateway that forwards requests to the Identity API.");

        group.MapPost("/login/start", StartLogin);
        group.MapPost("/login", Login);
        group.MapPost("/logout", Logout);
        group.MapPost("/create", CreateUser);
    }

    private const string RefreshTokenCookieName = "refreshToken"; // Måste ändras till rätt namn när token service är klar

    private static async Task<IResult> StartLogin(IHttpClientFactory factory, HttpContext context, StartLoginRequest request, CancellationToken ct)
    {
        var confirmed = await CheckEmailConfirmedAsync(factory, context, request.Email, ct);

        if (confirmed is null)
        {
            return Results.Problem("Email verification service failed.", statusCode: StatusCodes.Status502BadGateway);
        }

        // StartLogin är UI-steg: returnera statusen så frontend kan visa nästa steg eller felmeddelande
        return Results.Ok(new StartLoginResponse(request.Email, confirmed.Value));
    }

    private static async Task<IResult> Login(IHttpClientFactory factory, HttpContext context, LoginRequest request, CancellationToken ct)
    {
        var confirmed = await CheckEmailConfirmedAsync(factory, context, request.Email, ct);

        if (confirmed is null)
        {
            return Results.Problem("Email verification service failed.", statusCode: StatusCodes.Status502BadGateway);
        }

        if (confirmed.Value is false)
        {
            // Samma idé som i ert Identity API (EmailNotConfirmed => Forbid)
            return Results.Forbid();
        }

        // Email confirmed => fortsätt med “riktiga” login mot Identity API
        return await ForwardJsonAsync(factory, context, HttpMethod.Post, "/api/auth/login", request, ct);
    }

    private static async Task<bool?> CheckEmailConfirmedAsync(IHttpClientFactory factory, HttpContext context, string email, CancellationToken ct)
    {
        var client = factory.CreateClient(EmailVerificationHttpClientName);

        // TODO: uppdatera till er riktiga route i email verification-servicen
        var upstreamPath = $"/api/email/status?email={Uri.EscapeDataString(email)}";

        using var upstreamRequest = new HttpRequestMessage(HttpMethod.Get, upstreamPath);

        // Forwarda bara request-id (så ni kan spåra i loggar)
        if (context.Request.Headers.TryGetValue(RequestIdHeaderName, out var requestId))
        {
            _ = upstreamRequest.Headers.TryAddWithoutValidation(RequestIdHeaderName, requestId.ToString());
        }

        using var response = await client.SendAsync(upstreamRequest, ct);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return true;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        // Något oväntat (500, 503, 401, etc) => behandla som upstream-fel
        return null;
    }

    private static IResult Logout(HttpContext context)
    {
        context.Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None
        });

        return Results.NoContent();
    }

    private static Task<IResult> CreateUser(IHttpClientFactory factory, HttpContext context, CreateUserRequest request, CancellationToken ct)
        => ForwardJsonAsync(factory, context, HttpMethod.Post, "/api/auth/create", request, ct);

    private static async Task<IResult> ForwardJsonAsync<T>(IHttpClientFactory factory, HttpContext context, HttpMethod method, string upstreamPath, T body, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await ForwardAsync(factory, context, method, upstreamPath, content, ct);
    }

    private static async Task<IResult> ForwardAsync(IHttpClientFactory factory, HttpContext context, HttpMethod method, string upstreamPath, HttpContent? content, CancellationToken ct)
    {
        var client = factory.CreateClient(IdentityHttpClientName);

        using var upstreamRequest = new HttpRequestMessage(method, upstreamPath)
        {
            Content = content
        };

        CopyRequestHeaders(context, upstreamRequest);

        using var upstreamResponse = await client.SendAsync(
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

    private static void CopyRequestHeaders(HttpContext context, HttpRequestMessage upstreamRequest)
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

    private static void CopyResponseHeaders(HttpContext context, HttpResponseMessage upstreamResponse)
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
