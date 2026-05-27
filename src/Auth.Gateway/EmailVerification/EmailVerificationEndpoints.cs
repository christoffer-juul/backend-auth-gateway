using Auth.Gateway.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Auth.Gateway.EmailVerification;

public static partial class EmailVerificationEndpoints
{
    public static void MapEmailVerificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/email")
            .WithTags("EmailVerification");

        group.MapPost("/request-code", RequestCode);
        group.MapPost("/verify-code", VerifyCode);
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

    private static async Task<IResult> VerifyCode([FromBody] VerifyCodeRequest request, EmailVerificationApiClient emailClient, IdentityApiClient identityClient, HttpContext context, CancellationToken ct)
    {
        using var response = await emailClient.VerifyCodeAsync(request.Email, request.Code, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            return Results.Problem($"Azure Function returned {response.StatusCode}: {body}");
        }

        var content = JsonContent.Create(new
        {
            email = request.Email
        });

        return await identityClient.ForwardAsync(
            context,
            HttpMethod.Post,
            "/api/auth/confirm-email",
            content,
            ct);
    }
}