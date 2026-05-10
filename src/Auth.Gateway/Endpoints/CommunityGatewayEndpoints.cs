namespace Auth.Gateway.Endpoints;

public static class CommunityGatewayEndpoints
{
    public static void MapCommunityGatewayEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/communities")
            .WithTags("Communities");

        group.MapGet("/all", GetCommunities);
    }

    private static async Task<IResult> GetCommunities(IHttpClientFactory factory, HttpContext context)
    {
        var client = factory.CreateClient("CommunityApi");

        var request = new HttpRequestMessage(HttpMethod.Get, "api/communities/all");

        if (context.Request.Headers.ContainsKey("Authorization"))
        {
            request.Headers.Add("Authorization", context.Request.Headers.Authorization.ToString());
        }

        var response = await client.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        return Results.Text(content, "application/json", statusCode: (int)response.StatusCode);
    }
}
