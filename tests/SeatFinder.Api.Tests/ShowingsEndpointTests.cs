using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SeatFinder.Api.Contracts;

namespace SeatFinder.Api.Tests;

public class ShowingsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    // Noon UTC on 2026-12-01 is 6 AM in Nashville, so "today" is 2026-12-01 there too.
    private static readonly DateTimeOffset Now = new(2026, 12, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly WebApplicationFactory<Program> _factory;

    public ShowingsEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Replace the real clock so these tests mean the same thing on any day they run.
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddSingleton<TimeProvider>(new FixedTimeProvider(Now))));
    }

    [Fact]
    public async Task ValidRange_Returns200WithShowingsInRange()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/showings?from=2026-12-01&to=2026-12-02");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var showings = await response.Content.ReadFromJsonAsync<List<ShowingSummary>>();
        Assert.NotNull(showings);
        Assert.NotEmpty(showings);
        Assert.All(showings, s =>
        {
            Assert.InRange(DateOnly.FromDateTime(s.StartsAt.DateTime), new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 2));
            Assert.InRange(s.OpenSeats, 0, s.TotalSeats);
            Assert.False(string.IsNullOrWhiteSpace(s.TheaterName));
        });
    }

    [Theory]
    [InlineData("/showings?from=2026-12-05&to=2026-12-04", "to")]   // from after to
    [InlineData("/showings?from=2026-12-01&to=2026-12-15", "to")]   // 15 days
    [InlineData("/showings?from=2026-11-30&to=2026-12-02", "from")] // starts yesterday
    [InlineData("/showings?to=2026-12-02", "from")]                 // missing from
    [InlineData("/showings?from=2026-12-01", "to")]                 // missing to
    [InlineData("/showings?from=next-friday&to=2026-12-02", "from")] // not a date
    public async Task BadRange_Returns400NamingTheField(string url, string field)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(problem.Errors.Keys, key => key.Equals(field, StringComparison.OrdinalIgnoreCase));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
