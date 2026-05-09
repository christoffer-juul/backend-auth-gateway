namespace Auth.Gateway.OpenApi;

public static class OpenApiConfiguration
{
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<OpenApiDocumentTransformer>();
        });

        return services;
    }
}
