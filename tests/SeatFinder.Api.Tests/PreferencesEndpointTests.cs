using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SeatFinder.Api.Contracts;
using SeatFinder.Api.Data;

namespace SeatFinder.Api.Tests;

public class PreferencesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PreferencesEndpointTests(WebApplicationFactory<Program> factory)
    {
        // A fresh in-memory database per test class instance (xUnit makes one per test), so tests
        // don't see each other's saves and no Postgres is needed to run them.
        var databaseName = Guid.NewGuid().ToString();
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                // Remove the Postgres setup from Program.cs before adding the in-memory one;
                // EF Core refuses to run with two database providers registered.
                services.RemoveAll<DbContextOptions<SeatFinderDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<SeatFinderDbContext>>();
                services.AddDbContext<SeatFinderDbContext>(options => options.UseInMemoryDatabase(databaseName));
            }));
    }

    private static Dictionary<string, object?> ValidBody() => new()
    {
        ["preferredRowFrom"] = "E",
        ["preferredRowTo"] = "J",
        ["centerSeatFrom"] = 7,
        ["centerSeatTo"] = 14,
        ["avoidAisles"] = true,
        ["partySize"] = 3,
        ["searchRadiusMiles"] = 20,
        ["homeTheaterId"] = 2,
    };

    [Fact]
    public async Task Get_BeforeAnySave_ReturnsDefaults()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/preferences");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var preferences = await response.Content.ReadFromJsonAsync<PreferencesResponse>();
        var defaults = SeatPreference.CreateDefault();
        Assert.Equal(defaults.PartySize, preferences!.PartySize);
        Assert.Equal(defaults.PreferredRowFrom, preferences.PreferredRowFrom);
    }

    [Fact]
    public async Task Put_ThenGet_ReturnsWhatWasSaved()
    {
        var client = _factory.CreateClient();

        var put = await client.PutAsJsonAsync("/preferences", ValidBody());
        var get = await client.GetFromJsonAsync<PreferencesResponse>("/preferences");

        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        Assert.Equal(new PreferencesResponse("E", "J", 7, 14, true, 3, 20, 2), get);
    }

    [Fact]
    public async Task SecondPut_ReplacesTheFirst()
    {
        var client = _factory.CreateClient();
        var second = ValidBody();
        second["partySize"] = 5;
        second["homeTheaterId"] = null;

        await client.PutAsJsonAsync("/preferences", ValidBody());
        await client.PutAsJsonAsync("/preferences", second);
        var get = await client.GetFromJsonAsync<PreferencesResponse>("/preferences");

        Assert.Equal(5, get!.PartySize);
        Assert.Null(get.HomeTheaterId);
        using var scope = _factory.Services.CreateScope();
        Assert.Equal(1, await scope.ServiceProvider.GetRequiredService<SeatFinderDbContext>().SeatPreferences.CountAsync());
    }

    [Fact]
    public async Task Put_LowercaseRows_AreSavedUppercase()
    {
        var client = _factory.CreateClient();
        var body = ValidBody();
        body["preferredRowFrom"] = " e ";
        body["preferredRowTo"] = "j";

        var response = await client.PutAsJsonAsync("/preferences", body);

        var saved = await response.Content.ReadFromJsonAsync<PreferencesResponse>();
        Assert.Equal(("E", "J"), (saved!.PreferredRowFrom, saved.PreferredRowTo));
    }

    [Theory]
    [InlineData("partySize", 0)]               // below 1
    [InlineData("partySize", 9)]               // above 8
    [InlineData("searchRadiusMiles", 0)]       // below 1
    [InlineData("searchRadiusMiles", 51)]      // above 50
    [InlineData("preferredRowFrom", "Z")]      // no auditorium has row Z
    [InlineData("preferredRowTo", "B")]        // in front of row E
    [InlineData("centerSeatFrom", 0)]          // seats start at 1
    [InlineData("centerSeatTo", 3)]            // before seat 7
    [InlineData("homeTheaterId", 99)]          // unknown theater
    public async Task Put_BadValue_Returns400NamingTheField(string field, object value)
    {
        var client = _factory.CreateClient();
        var body = ValidBody();
        body[field] = value;

        var response = await client.PutAsJsonAsync("/preferences", body);

        await AssertBadRequestNaming(response, field);
    }

    [Theory]
    [InlineData("preferredRowFrom")]
    [InlineData("centerSeatTo")]
    [InlineData("avoidAisles")]
    [InlineData("partySize")]
    [InlineData("searchRadiusMiles")]
    public async Task Put_MissingField_Returns400NamingTheField(string field)
    {
        var client = _factory.CreateClient();
        var body = ValidBody();
        body.Remove(field);

        var response = await client.PutAsJsonAsync("/preferences", body);

        await AssertBadRequestNaming(response, field);
    }

    [Fact]
    public async Task Put_WrongType_Returns400NamingTheField()
    {
        var client = _factory.CreateClient();
        var body = ValidBody();
        body["partySize"] = "two";

        var response = await client.PutAsJsonAsync("/preferences", body);

        // JSON errors are keyed by path, e.g. "$.partySize".
        await AssertBadRequestNaming(response, "$.partySize");
    }

    [Fact]
    public async Task Put_NotJson_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsync("/preferences", new StringContent("party of four", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_Rejected_SavesNothing()
    {
        var client = _factory.CreateClient();
        var body = ValidBody();
        body["homeTheaterId"] = 99;

        await client.PutAsJsonAsync("/preferences", body);
        var get = await client.GetFromJsonAsync<PreferencesResponse>("/preferences");

        Assert.Equal(SeatPreference.CreateDefault().PartySize, get!.PartySize);
    }

    private static async Task AssertBadRequestNaming(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        // Exact match: the front end looks errors up by the JSON field name, so "PartySize" would be a miss.
        Assert.Contains(field, problem.Errors.Keys);
    }
}
