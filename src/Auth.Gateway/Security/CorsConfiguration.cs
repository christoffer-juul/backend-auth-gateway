namespace Auth.Gateway.Security;

public static class CorsConfiguration
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("All", policy =>
            {
                policy
                    .WithOrigins("http://localhost:3000", "https://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });


        return services;
    }
}
