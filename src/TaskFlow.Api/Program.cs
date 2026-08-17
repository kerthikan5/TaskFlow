using TaskFlow.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────
// Services
// ─────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Infrastructure: registers AppDbContext + PostgreSQL
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

// Health check — returns 200 OK, no DB dependency yet
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .WithName("HealthCheck")
   .AllowAnonymous();

app.Run();
