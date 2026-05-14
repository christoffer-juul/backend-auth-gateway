using System.Net;

namespace Auth.Gateway.Clients;

public class EmailVerificationApiClient
{
    private readonly HttpClient _client;

    public EmailVerificationApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<bool?> CheckEmailConfirmedAsync(string email, CancellationToken ct)
    {
        var upstreamPath = $"/api/auth/email-status?email={Uri.EscapeDataString(email)}";

        using var response = await _client.GetAsync(upstreamPath, ct);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return true;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return false;
        }

        return null;
    }
}