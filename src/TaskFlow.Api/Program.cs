using TaskFlow.Application.Extensions;
using TaskFlow.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────
// Services
// ─────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register Application & Infrastructure layer services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ─────────────────────────────────────────────────────────
// Middleware pipeline
// ─────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health check — returns 200 OK
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .WithName("HealthCheck")
   .AllowAnonymous();

app.Run();
