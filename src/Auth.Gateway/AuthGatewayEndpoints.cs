using Auth.Gateway.Authentication.Endpoints;
using Auth.Gateway.EmailVerification.Endpoints;
using Auth.Gateway.Identity.Endpoints;

namespace Auth.Gateway;

public static class AuthGatewayEndpoints
{
    public static void MapAuthGatewayEndpoints(this WebApplication app)
    {
        app.MapLoginEndpoints();
        app.MapLogoutEndpoints();
        app.MapAdminUserEndpoints();
        app.MapEmailVerificationEndpoints();
        app.MapUserEndpoints();
    }
}
