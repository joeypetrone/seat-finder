using SeatFinder.Api.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Singletons: the clock has no state, and the mock builds its fixed data once and only reads it after.
// Tests swap the clock for a fixed date so "not in the past" checks don't drift over time.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IShowtimeProvider, MockShowtimeProvider>();

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
