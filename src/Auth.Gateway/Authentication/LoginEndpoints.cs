using Auth.Gateway.EmailVerification;
using Auth.Gateway.Identity;
using Azure.Messaging.ServiceBus;
using System.Net;

namespace Auth.Gateway.Authentication;

public static class LoginEndpoints
{
    public static void MapLoginEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/login", Login);
    }

    private static async Task<IResult> Login(ServiceBusClient serviceBusClient, IConfiguration config, IdentityApiClient identityClient, JwtApiClient jwtClient, HttpContext context, LoginRequest request, CancellationToken ct)
    {
        using var response = await identityClient.CheckEmailConfirmedAsync(request.Email, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Results.NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            return Results.StatusCode((int)response.StatusCode);
        }

        var verifyResponse = await response.Content.ReadFromJsonAsync<VerifyEmailResponse>(ct);

        if (verifyResponse is null)
        {
            return Results.Problem("Failed to read email verification status.");
        }

        if (!verifyResponse.Confirmed)
        {
            var verificationRequest =
            new RequestCodeRequest(request.Email);

            await EmailVerificationEndpoints.RequestCode(serviceBusClient, verificationRequest, config, ct);

            return Results.Unauthorized();
        }

        using var loginResponse = await identityClient.PostLoginAsync(request.Email, request.Password, ct);

        if (loginResponse.StatusCode == HttpStatusCode.Unauthorized)
        {
            return Results.Json(new LoginResponse(Success: false, ErrorMessage: "Invalid email or password."), statusCode: StatusCodes.Status401Unauthorized);
        }

        if (!loginResponse.IsSuccessStatusCode)
            return Results.Problem("Login failed.");

        var identityUser = await loginResponse.Content.ReadFromJsonAsync<IdentityUserDto>(cancellationToken: ct);

        if (identityUser == null || string.IsNullOrEmpty(identityUser.UserId))
        {
            return Results.Problem("Identity API did not return user info.");
        }

        using var tokenResponse = await jwtClient.GenerateTokenAsync(request.Email, identityUser.UserId, identityUser.Roles, ct);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            return Results.Problem("Failed to generate JWT token.");
        }

        var jwtData = await tokenResponse.Content.ReadFromJsonAsync<GenerateJwtResponse>(cancellationToken: ct);

        if(jwtData is null)
        {
            return Results.Problem("Failed to read JWT token from response.");
        }

        return Results.Ok(new LoginResponse(Success: true, Token: jwtData.Token));
    }
}

