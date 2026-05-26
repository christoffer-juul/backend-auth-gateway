using Microsoft.AspNetCore.Mvc;

namespace Auth.Gateway.Identity;

public static class AdminUserEndpoints
{
    public static void MapAdminUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .RequireAuthorization(policy => policy.RequireRole("admin"));

        group.MapPost("/users", CreateUser);
        group.MapDelete("/users/{id}", DeleteUser);
    }

    private static async Task<IResult> CreateUser(IdentityApiClient identityClient, HttpContext context, CreateUserRequest request, CancellationToken ct)
        => await identityClient.ForwardJsonAsync(context, HttpMethod.Post, "/api/admin/users", request, ct);

    private static async Task<IResult> DeleteUser(IdentityApiClient identityClient, HttpContext context, [FromRoute] string id, CancellationToken ct)
        => await identityClient.ForwardAsync(context, HttpMethod.Delete, $"/api/admin/users/{id}", null, ct);
}
