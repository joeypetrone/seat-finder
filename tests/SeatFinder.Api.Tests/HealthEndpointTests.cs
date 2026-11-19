using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SeatFinder.Api.Tests;

// Ask xUnit for one in-memory copy of the API, shared by every test in this class.
public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Health_Returns200AndHealthy()
    {
        // Arrange: an HttpClient wired straight into the in-memory API (no real port).
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert: only a 200 with exactly "Healthy" counts (same rule as the front end).
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", body);
    }
}
