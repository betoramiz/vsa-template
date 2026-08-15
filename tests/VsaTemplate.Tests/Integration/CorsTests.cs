using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using VsaTemplate.Tests.Common;

namespace VsaTemplate.Tests.Integration;

public class CorsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task In_development_any_origin_is_allowed()
    {
        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("Origin", "http://anything.example");

        var response = await client.SendAsync(request);

        Assert.Equal("*", GetAllowOriginHeader(response));
    }

    [Fact]
    public async Task In_production_only_configured_origins_are_allowed()
    {
        var client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
                builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
                [
                    new("Cors:AllowedOrigins:0", "https://allowed.example"),
                ]));
            })
            .CreateClient();

        var allowedRequest = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        allowedRequest.Headers.Add("Origin", "https://allowed.example");
        var allowedResponse = await client.SendAsync(allowedRequest);
        Assert.Equal("https://allowed.example", GetAllowOriginHeader(allowedResponse));

        var blockedRequest = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        blockedRequest.Headers.Add("Origin", "https://not-allowed.example");
        var blockedResponse = await client.SendAsync(blockedRequest);
        Assert.Null(GetAllowOriginHeader(blockedResponse));
    }

    private static string? GetAllowOriginHeader(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values) ? values.FirstOrDefault() : null;
}
