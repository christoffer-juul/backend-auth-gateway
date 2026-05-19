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

    private static async Task<IResult> Login(ServiceBusClient serviceBusClient, IConfiguration config, IdentityApiClient identityClient, EmailVerificationApiClient emailClient, HttpContext context, LoginRequest request, CancellationToken ct)
    {
        using var response = await emailClient.CheckEmailConfirmedAsync(request.Email, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
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

