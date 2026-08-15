var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────
// Services
// ─────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

// Health check endpoint — simple, no dependencies
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .WithName("HealthCheck")
   .AllowAnonymous();

app.Run();
