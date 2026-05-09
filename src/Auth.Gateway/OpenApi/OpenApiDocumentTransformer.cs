using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Auth.Gateway.OpenApi;

public sealed class OpenApiDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();

        document.Info.Description = """
        ## Introduction

        The Authentication Gateway API serves as the central entry point for authentication 
        and authorization across the platform.

        It is responsible for managing secure communication between the frontend application 
        and backend services, while handling identity and access control operations.

        ### Features

        - User authentication and login
        - JWT and access token validation
        - Authorization and protected route handling
        - Secure communication between services
        - Centralized access management

        The gateway simplifies security management by providing a unified authentication layer 
        for all connected services and applications.
        """;

        return Task.CompletedTask;
    }
}
