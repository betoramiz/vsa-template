using System.Net;
using Microsoft.Extensions.Configuration;
using VsaTemplate.Tests.Common;

namespace VsaTemplate.Tests.Integration;

public class RateLimitingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Exceeding_the_configured_limit_returns_429()
    {
        var client = factory
            .WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
            [
                new("RateLimiting:PermitLimit", "2"),
                new("RateLimiting:WindowSeconds", "60"),
            ])))
            .CreateClient();

        for (var i = 0; i < 2; i++)
            Assert.NotEqual(HttpStatusCode.TooManyRequests, (await client.GetAsync("/products")).StatusCode);

        var response = await client.GetAsync("/products");

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task Health_endpoints_are_exempt_from_the_limit()
    {
        var client = factory
            .WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
            [
                new("RateLimiting:PermitLimit", "1"),
                new("RateLimiting:WindowSeconds", "60"),
            ])))
            .CreateClient();

        for (var i = 0; i < 5; i++)
        {
            var response = await client.GetAsync("/health/live");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
