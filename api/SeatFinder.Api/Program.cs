var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

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
