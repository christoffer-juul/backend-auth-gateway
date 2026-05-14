using Auth.Gateway.Dtos;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Auth.Gateway.Clients;

namespace Auth.Gateway.Endpoints;

public static partial class EmailVerificationEndpoints
{
    private const string EmailVerificationHttpClientName = "EmailVerificationApi";

    public static void MapEmailVerificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/email")
            .WithTags("EmailVerification");

        group.MapPost("/request-code", RequestCode);
        group.MapPost("/verify", VerifyCode);
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

    private static async Task<IResult> VerifyCode(IHttpClientFactory factory, IConfiguration config, [FromBody] VerifyCodeRequest request, IdentityApiClient identityClient, HttpContext context, CancellationToken ct)
    {
        var client = factory.CreateClient(EmailVerificationHttpClientName);
        var upstreamPath = "/api/VerifyCode";

        var functionKey = config["EmailVerificationApi:FunctionKey"]; 
        
        if (!string.IsNullOrEmpty(functionKey))
        {
            client.DefaultRequestHeaders.Add("x-functions-key", functionKey);
        }

        var response = await client.PostAsJsonAsync(upstreamPath, new { email = request.Email, code = request.Code }, ct);

        if (!response.IsSuccessStatusCode)
        {
            return Results.BadRequest(new { Message = "Verifikationskoden var fel eller ogiltig." });
        }

        return await identityClient.ForwardAsync(
            context, 
            HttpMethod.Post, 
            $"/confirm-email?email={Uri.EscapeDataString(request.Email)}", 
            null, 
            ct);
    }
}