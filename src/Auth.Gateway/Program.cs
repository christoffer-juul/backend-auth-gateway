using Auth.Gateway.Endpoints;
using Auth.Gateway.OpenApi;
using Auth.Gateway.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCorsConfiguration();
builder.Services.AddOpenApiConfiguration();

var app = builder.Build();

app.UseCors("All");
app.UseHttpsRedirection();

app.MapOpenApiEndpoints();
app.MapAuthGatewayEndpoints();

app.Run();
