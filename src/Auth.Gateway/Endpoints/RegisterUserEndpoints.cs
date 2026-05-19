using Auth.Gateway.Dtos;

namespace Auth.Gateway.Endpoints;

public static class RegisterUserEndpoints
{
    public static void MapRegisterUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", RegisterUser);
    }

    private static Task<IResult> RegisterUser(Auth.Gateway.Clients.IdentityApiClient identityClient, HttpContext context, CreateUserRequest request, CancellationToken ct)
        => identityClient.ForwardJsonAsync(context, HttpMethod.Post, "/api/auth/create", request, ct);
}
