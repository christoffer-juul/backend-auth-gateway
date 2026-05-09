using Scalar.AspNetCore;

namespace Auth.Gateway.OpenApi;

public static class OpenApiEndpointsExtensions
{
    public static WebApplication MapOpenApiEndpoints(this WebApplication app)
    {
        //if (!app.Environment.IsDevelopment())
        //    return app;

        app.MapOpenApi();

        app.MapScalarApiReference("/docs", options => options
            .WithTitle("Documentation for the Authentication Gateway API"));

        return app;
    }
}
