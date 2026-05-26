using System.Net;

namespace Auth.Gateway.EmailVerification;

public class EmailVerificationApiClient
{
    private readonly HttpClient _client;

    public EmailVerificationApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> VerifyCodeAsync(string email, string code, CancellationToken ct)
    {
        var upstreamPath = "/api/VerifyCode";

        Console.WriteLine($"Calling Azure Function: {_client.BaseAddress}{upstreamPath}");
        Console.WriteLine($"With email: {email}, code: {code}");
        Console.WriteLine($"Function key header present: {_client.DefaultRequestHeaders.Contains("x-functions-key")}");

        return await _client.PostAsJsonAsync(upstreamPath, new { email, code }, ct);
    }
}