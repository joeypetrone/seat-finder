using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using SeatFinder.Api.Data;
using SeatFinder.Api.Providers;

var builder = WebApplication.CreateBuilder(args);

// Name body validation errors by their JSON field ("partySize"), not the C# property ("PartySize"),
// so the front end can match an error to the input it came from.
builder.Services.AddControllers(options =>
    options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider()));
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Singletons: the clock has no state, and the mock builds its fixed data once and only reads it after.
// Tests swap the clock for a fixed date so "not in the past" checks don't drift over time.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IShowtimeProvider, MockShowtimeProvider>();

// The connection string lives in user secrets, never in appsettings. It's read when a DbContext is
// first created rather than at startup, so endpoints that don't touch the database (and the tests,
// which swap in an in-memory database) run without one.
builder.Services.AddDbContext<SeatFinderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SeatFinder")
        ?? throw new InvalidOperationException(
            "Connection string 'SeatFinder' is missing. Set it with: " +
            "dotnet user-secrets set \"ConnectionStrings:SeatFinder\" \"<connection string>\" --project api/SeatFinder.Api")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// No UseHttpsRedirection: the API only runs locally over http (port 5166); the live demo is static files.

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

// Lets integration tests host the app with WebApplicationFactory<Program>.
public partial class Program { }
