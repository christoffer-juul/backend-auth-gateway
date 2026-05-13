using Auth.Gateway.Dtos;
using Auth.Gateway.Helpers;

public static class LoginEndpoints
{
    public static void MapLoginEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/login/start", StartLogin);
        group.MapPost("/login", Login);
    }

    private static async Task<IResult> StartLogin(Auth.Gateway.Clients.EmailVerificationApiClient emailClient, HttpContext context, StartLoginRequest request, CancellationToken ct)
    {
        var confirmed = await emailClient.CheckEmailConfirmedAsync(request.Email, ct);

        if (confirmed is null)
        {
            return Results.Problem("Email verification service failed.", statusCode: StatusCodes.Status502BadGateway);
        }

        return Results.Ok(new StartLoginResponse(request.Email, confirmed.Value));
    }

    private static async Task<IResult> Login(Auth.Gateway.Clients.IdentityApiClient identityClient, Auth.Gateway.Clients.EmailVerificationApiClient emailClient, HttpContext context, LoginRequest request, CancellationToken ct)
    {
        var confirmed = await emailClient.CheckEmailConfirmedAsync(request.Email, ct);

        if (confirmed is null)
        {
            return Results.Problem("Email verification service failed.", statusCode: StatusCodes.Status502BadGateway);
        }

        if (confirmed.Value is false)
        {
            //Skicka till servicebus så ett verification mail skickas ut
        }

        return await identityClient.ForwardJsonAsync(context, HttpMethod.Post, "/api/auth/login", request, ct);
    }
}

