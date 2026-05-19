using Auth.Gateway.Dtos;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Auth.Gateway.Clients;
using Azure;

namespace Auth.Gateway.Endpoints;

public static partial class EmailVerificationEndpoints
{
    public static void MapEmailVerificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/email")
            .WithTags("EmailVerification");

        group.MapPost("/verify", VerifyEmail);
        group.MapPost("/request-code", RequestCode);
        group.MapPost("/verify-code", VerifyCode);
    }

    private static async Task<IResult> VerifyEmail(IdentityApiClient identityClient, HttpContext context, VerifyEmailRequest request, CancellationToken ct)
    {
        using var response = await identityClient.CheckEmailConfirmedAsync(request.Email, ct);

        if (!response.IsSuccessStatusCode)
        {
            return Results.StatusCode((int)response.StatusCode);
        }

        return Results.Ok(new VerifyEmailResponse(request.Email, true));
    }

    public static async Task<IResult> RequestCode(ServiceBusClient serviceBusClient, [FromBody] RequestCodeRequest request, IConfiguration config, CancellationToken ct)
    {
        var queueName = config["ServiceBus:QueueName"] ?? "email-verification";
        
        await using var sender = serviceBusClient.CreateSender(queueName);

        var messagePayload = JsonSerializer.Serialize(new { email = request.Email });
        var sbMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(messagePayload));

        await sender.SendMessageAsync(sbMessage, ct);

        return Results.Ok();
    }

    private static async Task<IResult> VerifyCode(EmailVerificationApiClient emailClient, [FromBody] VerifyCodeRequest request, IdentityApiClient identityClient, HttpContext context, CancellationToken ct)
    {
        using var response = await emailClient.VerifyCodeAsync(request.Email, request.Code, ct);

        if (!response.IsSuccessStatusCode)
        {
            return Results.StatusCode((int)response.StatusCode);
        }

        return await identityClient.ForwardAsync(
            context, 
            HttpMethod.Post, 
            $"/confirm-email?email={Uri.EscapeDataString(request.Email)}", 
            null, 
            ct);
    }
}