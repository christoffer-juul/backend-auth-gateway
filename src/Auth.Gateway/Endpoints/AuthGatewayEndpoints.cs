namespace Auth.Gateway.Endpoints;

public static class AuthGatewayEndpoints
{
    public static void MapAuthGatewayEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithDescription("Provides authentication and authorization endpoints for the platform. " +
            "This gateway handles:" +
            "- User authentication and login" +
            "- JWT/token validation" +
            "- Authorization flows" +
            "- Secure communication between the frontend and backend services. " +
            "The Authentication API acts as a central entry point for managing user identity and access control.");

        group.MapPost("/login", Login);
        group.MapPost("/refresh", RefreshToken);
        group.MapPost("/Logout", Logout);
        group.MapGet("/validate", ValidateToken);
        group.MapGet("/me", GetCurrentUser);
    }

    private static IResult Login() => Results.Ok();

    private static IResult RefreshToken() => Results.Ok();

    private static IResult Logout() => Results.Ok();

    private static IResult ValidateToken() => Results.Ok();

    private static IResult GetCurrentUser() => Results.Ok();
}
