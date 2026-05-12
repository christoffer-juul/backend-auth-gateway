using Auth.Gateway.EmailVerification;
using Auth.Gateway.Endpoints;
using Auth.Gateway.Identity;
using Auth.Gateway.OpenApi;
using Auth.Gateway.Security;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCorsConfiguration();
builder.Services.AddOpenApiConfiguration();

builder.Services.AddOptions<IdentityApiOptions>()
    .Bind(builder.Configuration.GetSection(IdentityApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient("IdentityApi", (sp, http) =>
{
    var options = sp.GetRequiredService<IOptions<IdentityApiOptions>>().Value;
    http.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    http.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddOptions<EmailVerificationApiOptions>()
    .Bind(builder.Configuration.GetSection(EmailVerificationApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient("EmailVerificationApi", (sp, http) =>
{
    var options = sp.GetRequiredService<IOptions<EmailVerificationApiOptions>>().Value;
    http.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    http.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

app.UseCors("All");
app.UseHttpsRedirection();

app.MapOpenApiEndpoints();
app.MapAuthGatewayEndpoints();

app.Run();
