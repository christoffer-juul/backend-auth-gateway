using Auth.Gateway.Dtos;
using Auth.Gateway.Endpoints;
using Auth.Gateway.Clients;
using Azure.Messaging.ServiceBus;

public static class LoginEndpoints
{
    public static void MapLoginEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/login/start", StartLogin);
        group.MapPost("/login", Login);
    }

    private static async Task<IResult> StartLogin(EmailVerificationApiClient emailClient, HttpContext context, StartLoginRequest request, CancellationToken ct)
    {
        var confirmed = await emailClient.CheckEmailConfirmedAsync(request.Email, ct);

        if (confirmed is null)
        {
            return Results.Problem("Email verification service failed.", statusCode: StatusCodes.Status502BadGateway);
        }

        return Results.Ok(new StartLoginResponse(request.Email, confirmed.Value));
    }

    private static async Task<IResult> Login(ServiceBusClient serviceBusClient, IConfiguration config, IdentityApiClient identityClient, EmailVerificationApiClient emailClient, HttpContext context, LoginRequest request, CancellationToken ct)
    {
        var confirmed = await emailClient.CheckEmailConfirmedAsync(request.Email, ct);

        if (confirmed is null)
        {
            return Results.Problem("Email verification service failed.", statusCode: StatusCodes.Status502BadGateway);
        }

        if (confirmed.Value is false)
        {
            var verificationRequest = new RequestCodeRequest(request.Email);

            await EmailVerificationEndpoints.RequestCode(serviceBusClient, verificationRequest, config, ct);

            return Results.Forbid();
        }

        return await identityClient.ForwardJsonAsync(context, HttpMethod.Post, "/api/auth/login", request, ct);
    }
}

