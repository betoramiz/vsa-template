using VsaTemplate.Tests.Common;

namespace VsaTemplate.Tests.Integration;

public class CorrelationIdTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Every_response_carries_an_X_Correlation_Id_header()
    {
        var response = await _client.GetAsync("/health/live");

        Assert.True(response.Headers.TryGetValues("X-Correlation-Id", out var values));
        Assert.NotEmpty(values!.First());
    }

    [Fact]
    public async Task Different_requests_get_different_correlation_ids()
    {
        var first = await _client.GetAsync("/health/live");
        var second = await _client.GetAsync("/health/live");

        var firstId = first.Headers.GetValues("X-Correlation-Id").First();
        var secondId = second.Headers.GetValues("X-Correlation-Id").First();

        Assert.NotEqual(firstId, secondId);
    }
}
