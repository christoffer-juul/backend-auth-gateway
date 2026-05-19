using System.Net;

namespace Auth.Gateway.Clients;

public class EmailVerificationApiClient
{
    private readonly HttpClient _client;

    public EmailVerificationApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> CheckEmailConfirmedAsync(string email, CancellationToken ct)
    {
        var upstreamPath = $"/api/auth/email-status?email={Uri.EscapeDataString(email)}";

        return await _client.GetAsync(upstreamPath, ct);
    }
}