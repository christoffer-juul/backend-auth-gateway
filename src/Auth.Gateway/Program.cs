using Auth.Gateway.Authentication;
using Auth.Gateway.EmailVerification;
using Auth.Gateway.Identity;
using Auth.Gateway.OpenApi;
using Auth.Gateway.Profile;
using Auth.Gateway.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCorsConfiguration();
builder.Services.AddOpenApiConfiguration();

builder.Services.AddOptions<IdentityApiOptions>()
    .Bind(builder.Configuration.GetSection(IdentityApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<IdentityApiClient>((sp, http) =>
{
    var options =
        sp.GetRequiredService<IOptions<IdentityApiOptions>>().Value;

    ConfigureApiClient(
        http,
        options.BaseUrl,
        options.ApiKey,
        TimeSpan.FromSeconds(30));
});

builder.Services.AddOptions<EmailVerificationApiOptions>()
    .Bind(builder.Configuration.GetSection(
        EmailVerificationApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<EmailVerificationApiClient>((sp, http) =>
{
    var options =
        sp.GetRequiredService<IOptions<EmailVerificationApiOptions>>().Value;

    ConfigureApiClient(
        http,
        options.BaseUrl,
        options.FunctionKey,
        TimeSpan.FromSeconds(10),
        "x-functions-key");
});

builder.Services.AddOptions<JwtApiOptions>()
    .Bind(builder.Configuration.GetSection(
        JwtApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<JwtApiClient>((sp, http) =>
{
    var options =
        sp.GetRequiredService<IOptions<JwtApiOptions>>().Value;

    ConfigureApiClient(
        http,
        options.BaseUrl,
        options.ApiKey,
        TimeSpan.FromSeconds(10));
});

builder.Services.AddOptions<ProfileApiOptions>()
    .Bind(builder.Configuration.GetSection(
        ProfileApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<ProfileApiClient>((sp, http) =>
{
    var options =
        sp.GetRequiredService<IOptions<ProfileApiOptions>>().Value;

    ConfigureApiClient(
        http,
        options.BaseUrl,
        options.ApiKey,
        TimeSpan.FromSeconds(10));
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:SigningKey"]!))
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddServiceBusClient(
        builder.Configuration.GetConnectionString("ServiceBus"));
});

Console.WriteLine("JWT Issuer: " + builder.Configuration["Jwt:Issuer"]);
Console.WriteLine("JWT Audience: " + builder.Configuration["Jwt:Audience"]);
Console.WriteLine("JWT SigningKey: " + builder.Configuration["Jwt:SigningKey"]);

var app = builder.Build();

app.UseCors("All");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApiEndpoints();
app.MapAuthGatewayEndpoints();

app.Run();

static void ConfigureApiClient(
    HttpClient http,
    string baseUrl,
    string? apiKey,
    TimeSpan timeout,
    string apiKeyHeader = "x-api-key")
{
    http.BaseAddress =
        new Uri(baseUrl, UriKind.Absolute);

    http.Timeout = timeout;

    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        http.DefaultRequestHeaders.Add(
            apiKeyHeader,
            apiKey);
    }
}