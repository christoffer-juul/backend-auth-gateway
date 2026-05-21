namespace Auth.Gateway.Identity;

public static class RegisterUserEndpoints
{
    public static void MapRegisterUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", RegisterUser);
    }

    private static async Task<IResult> RegisterUser(IdentityApiClient identityClient, HttpContext context, CreateUserRequest request, CancellationToken ct)
        => await identityClient.ForwardJsonAsync(context, HttpMethod.Post, "/api/auth/create", request, ct);
}
