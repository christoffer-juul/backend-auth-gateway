using Auth.Gateway.EmailVerification;
using Auth.Gateway.Endpoints;
using Auth.Gateway.Identity;
using Auth.Gateway.OpenApi;
using Auth.Gateway.Security;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCorsConfiguration();
builder.Services.AddOpenApiConfiguration();

builder.Services.AddOptions<IdentityApiOptions>()
    .Bind(builder.Configuration.GetSection(IdentityApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<Auth.Gateway.Clients.IdentityApiClient>((sp, http) =>
{
    var options = sp.GetRequiredService<IOptions<IdentityApiOptions>>().Value;
    http.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    http.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddOptions<EmailVerificationApiOptions>()
    .Bind(builder.Configuration.GetSection(EmailVerificationApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<Auth.Gateway.Clients.EmailVerificationApiClient>((sp, http) =>
{
    var options = sp.GetRequiredService<IOptions<EmailVerificationApiOptions>>().Value;
    
    http.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    http.Timeout = TimeSpan.FromSeconds(10);

    if (!string.IsNullOrEmpty(options.FunctionKey))
    {
        http.DefaultRequestHeaders.Add("x-functions-key", options.FunctionKey);
    }
});

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddServiceBusClient(builder.Configuration.GetConnectionString("ServiceBus"));
});

var app = builder.Build();

app.UseCors("All");
app.UseHttpsRedirection();

app.MapOpenApiEndpoints();
app.MapAuthGatewayEndpoints();

app.Run();
