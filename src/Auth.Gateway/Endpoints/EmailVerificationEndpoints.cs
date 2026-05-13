using System.Net;

namespace Auth.Gateway.Endpoints;

public static class EmailVerificationEndpoints
{
    public static void MapEmailVerificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/email");

        group.MapPost("/request-code", RequestCode);
        group.MapPost("/verify", VerifyCode);
    }

    private static IResult RequestCode() 
    {
        return Results.Ok();
    }

    private static IResult VerifyCode() 
    {
        return Results.Ok();
    }
}