using Auth.Gateway.Endpoints;
using Auth.Gateway.OpenApi;
using Auth.Gateway.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCorsConfiguration();
builder.Services.AddOpenApiConfiguration();

builder.Services.AddHttpClient("CommunityApi", client =>
{
    client.BaseAddress = new Uri("https://shiko-team1-backend-community-api.azurewebsites.net");
});

var app = builder.Build();

app.UseCors("All");
app.UseHttpsRedirection();

app.MapOpenApiEndpoints();
app.MapAuthGatewayEndpoints();
app.MapCommunityGatewayEndpoints();

app.Run();
