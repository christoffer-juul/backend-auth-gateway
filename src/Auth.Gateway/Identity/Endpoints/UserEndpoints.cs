using Auth.Gateway.EmailVerification.Responses;
using Auth.Gateway.Identity.Client;
using Auth.Gateway.Identity.Requests;
using Auth.Gateway.Identity.Responses;
using Auth.Gateway.Profile.Client;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace Auth.Gateway.Identity.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .RequireAuthorization();

        group.MapGet("/email-status", CheckEmailStatus);
        group.MapPost("/confirm-email", ConfirmEmail);
        group.MapPost("/set-password", SetPassword);
        group.MapPut("/change-password", ChangePassword);
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
        using var passwordResponse = await identityClient.SetPasswordAsync(
            context,
            new SetPasswordDto(request.Email, request.Password),
            ct);

        if (!passwordResponse.IsSuccessStatusCode)
        {
            return Results.StatusCode((int)passwordResponse.StatusCode);
        }

        var passwordResult = await passwordResponse.Content.ReadFromJsonAsync<SetPasswordResponse>(ct);

        if (passwordResult?.UserId is null)
        {
            return Results.Problem("Failed to retrieve user ID after setting password.");
        }

        using var namesResponse = await profileClient.SetNamesAsync(new ProfileDto
        {
            UserId = passwordResult.UserId,
            FirstName = request.FirstName,
            LastName = request.LastName
        }, ct);

        if (namesResponse.IsSuccessStatusCode)
        {
            return Results.Ok(new { status = "Ok" });
        }

        var status = namesResponse.StatusCode switch
        {
            HttpStatusCode.NotFound => "NotFound",
            HttpStatusCode.Conflict => "Conflict",
            HttpStatusCode.BadRequest => "BadRequest",
            _ => "Problem"
        };

        return Results.Json(
            new
            {
                status,
                error = "Failed to set names"
            },
            statusCode: (int)namesResponse.StatusCode);
    }

    private static async Task<IResult> ChangePassword(IdentityApiClient identityClient, HttpContext context, [FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var email = context.User.FindFirst(ClaimTypes.Email)?.Value;

        if (email is null)
            return Results.Unauthorized();

        var passwordResponse = await identityClient.ChangePasswordAsync(context, new ChangePasswordDto(email, request.CurrentPassword, request.NewPassword), ct);

        return passwordResponse.IsSuccessStatusCode
            ? Results.Ok()
            : Results.StatusCode((int)passwordResponse.StatusCode);
    }
}
