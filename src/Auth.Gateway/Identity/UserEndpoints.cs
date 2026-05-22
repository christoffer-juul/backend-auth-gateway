using Auth.Gateway.EmailVerification;
using Auth.Gateway.Profile;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Auth.Gateway.Identity;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapGet("/email-status", CheckEmailStatus);
        group.MapPost("/confirm-email", ConfirmEmail);
        group.MapPost("/set-password", SetPassword);
        group.MapPost("/change-password", ChangePassword);
    }

    private static async Task<IResult> ConfirmEmail(IdentityApiClient identityClient, HttpContext context, [FromBody] object payload, CancellationToken ct)
        => await identityClient.ForwardJsonAsync(context, HttpMethod.Post, "/api/auth/confirm-email", payload, ct);

    private static async Task<IResult> CheckEmailStatus(IdentityApiClient identityClient, [FromQuery] string email, CancellationToken ct)
    {
        using var response = await identityClient.CheckEmailConfirmedAsync(email, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Results.Json(
                new
                {
                    success = false,
                    message = "Invalid email address."
                },
                statusCode: StatusCodes.Status404NotFound);
        }

        if (!response.IsSuccessStatusCode)
        {
            return Results.Json(
                new
                {
                    success = false,
                    message = "Failed to check email status."
                },
                statusCode: (int)response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<VerifyEmailResponse>(ct);

        if (result is null)
        {
            return Results.Problem("Failed to parse email status response.");
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> SetPassword(IdentityApiClient identityClient, ProfileApiClient profileClient, HttpContext context, [FromBody] SetPasswordRequest request, CancellationToken ct)
    {
        var passwordResponse = await identityClient.SetPasswordAsync(
            context,
            new PasswordDto(request.Password),
            ct);

        if (!passwordResponse.IsSuccessStatusCode)
        {
            return Results.StatusCode((int)passwordResponse.StatusCode);
        }

        var namesResponse = await profileClient.SetNamesAsync(new ProfileDto
        {
            FirstName = request.FirstName,
            LastName = request.LastName
        }, ct);

        return namesResponse.IsSuccessStatusCode
            ? Results.Ok()
            : Results.StatusCode((int)namesResponse.StatusCode);
    }

    private static async Task<IResult> ChangePassword(IdentityApiClient identityClient, HttpContext context, [FromBody] object payload, CancellationToken ct)
        => await identityClient.ForwardJsonAsync(context, HttpMethod.Put, "/api/auth/gateway/change-password", payload, ct);
}
