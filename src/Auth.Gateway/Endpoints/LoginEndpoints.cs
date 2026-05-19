using Auth.Gateway.Dtos;
using Auth.Gateway.Endpoints;
using Auth.Gateway.Clients;
using Azure.Messaging.ServiceBus;

public static class LoginEndpoints
{
    public static void MapLoginEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/login", Login);
    }

    private static async Task<IResult> Login(ServiceBusClient serviceBusClient, IConfiguration config, IdentityApiClient identityClient, HttpContext context, LoginRequest request, CancellationToken ct)
    {
        using var response = await identityClient.CheckEmailConfirmedAsync(request.Email, ct);

        string? responseBody = response.Content != null ? await response.Content.ReadAsStringAsync(ct) : null;

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Results.NotFound();
        }

        if (responseBody != null && responseBody.Contains(""" "confirmed":false """, StringComparison.OrdinalIgnoreCase))
        {
            var verificationRequest = new RequestCodeRequest(request.Email);
            await EmailVerificationEndpoints.RequestCode(serviceBusClient, verificationRequest, config, ct);

            return Results.Forbid();
        }
        else if (!response.IsSuccessStatusCode)
        {
            return Results.StatusCode((int)response.StatusCode);
        }

        return await identityClient.ForwardJsonAsync(context, HttpMethod.Post, "/api/auth/login", request, ct);
    }
}

