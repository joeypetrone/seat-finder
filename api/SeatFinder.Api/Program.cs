// Top-level statements: this file *is* Main(). No Program class or Startup class needed.
var builder = WebApplication.CreateBuilder(args);

// 1. Register services with dependency injection (the "container").
builder.Services.AddControllers();
builder.Services.AddOpenApi();      // generates /openapi/v1.json (used later for TS codegen)
builder.Services.AddHealthChecks(); // built-in health check service

var app = builder.Build();

// 2. Configure the HTTP pipeline (middleware runs in this order for every request).
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// No UseHttpsRedirection: Cloud Run terminates HTTPS before requests reach the container.

app.UseAuthorization();

// 3. Map endpoints.
app.MapHealthChecks("/health"); // returns 200 "Healthy" when all checks pass
app.MapControllers();

app.Run();

// Makes Program visible to the test project (WebApplicationFactory<Program>) in Session 3.
public partial class Program { }
