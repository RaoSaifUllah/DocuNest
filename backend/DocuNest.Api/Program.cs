using System.Diagnostics;
using System.Threading.RateLimiting;
using DocuNest.Infrastructure.Configuration;
using DocuNest.Infrastructure.Persistence;
using DocuNest.Infrastructure.Extension;
using DocuNest.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
EnvironmentFile.LoadDevelopment(builder.Environment.ContentRootPath, builder.Environment.IsDevelopment());
builder.Configuration.AddEnvironmentVariables();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogLevel.None);
builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogLevel.Warning);
builder.Services.Configure<RuntimeOptions>(builder.Configuration.GetSection("DocuNest"));
var write = builder.Configuration.GetConnectionString("Write") ?? throw new InvalidOperationException("ConnectionStrings:Write is required.");
builder.Services.AddDocuNestPersistence(write, builder.Configuration.GetConnectionString("Read"));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    context.ProblemDetails.Extensions["correlationId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
    if (!context.ProblemDetails.Extensions.ContainsKey("code")) context.ProblemDetails.Extensions["code"] = $"http_{context.HttpContext.Response.StatusCode}";
});
builder.Services.AddExceptionHandler<SafeExceptionHandler>();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("postgresql");
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("auth", limiter => { limiter.PermitLimit = 10; limiter.Window = TimeSpan.FromMinutes(1); limiter.QueueLimit = 0; });
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await Results.Problem(statusCode: 429, title: "Request limit reached", extensions: new Dictionary<string, object?> { ["code"] = "rate_limit", ["correlationId"] = context.HttpContext.TraceIdentifier }).ExecuteAsync(context.HttpContext);
    };
});
var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
    context.Response.Headers.CacheControl = "no-store";
    await next();
});
app.UseRateLimiter();
app.MapHealthChecks("/health/ready");
app.MapGet("/health/live", () => Results.Ok(new { status = "alive" }));
app.MapGet("/api/v1/capabilities", () => Results.Ok(new { formats = new[] { "pdf", "jpeg", "png", "webp", "tiff" }, maxFileBytes = 50 * 1024 * 1024, camera = "browser", offline = "shell-only", notifications = "local-only" }));
if (app.Environment.IsDevelopment()) app.MapOpenApi();
if (args.Contains("--migrate", StringComparer.Ordinal) || args.Contains("--database-check", StringComparer.Ordinal))
{
    var migration = builder.Configuration.GetConnectionString("Migration") ?? write;
    var cs = new Npgsql.NpgsqlConnectionStringBuilder(migration);
    var schema = cs.SearchPath?.Split(',')[0].Trim() ?? "public";
    if (!System.Text.RegularExpressions.Regex.IsMatch(schema, "^[a-z][a-z0-9_]{0,62}$")) throw new InvalidOperationException("Use a simple lowercase schema name.");
    await using var connection = new Npgsql.NpgsqlConnection(migration);
    await connection.OpenAsync();
    await using (var command = new Npgsql.NpgsqlCommand($"CREATE SCHEMA IF NOT EXISTS \"{schema}\"", connection)) await command.ExecuteNonQueryAsync();
    await using (var command = new Npgsql.NpgsqlCommand($"SET search_path TO \"{schema}\", public", connection)) await command.ExecuteNonQueryAsync();
    await using (var command = new Npgsql.NpgsqlCommand("SELECT current_schema()", connection))
    {
        var current = (string?)await command.ExecuteScalarAsync();
        if (current != schema) throw new InvalidOperationException("The database connection did not select the configured schema; migration refused.");
        Console.WriteLine($"Database schema verified: {current}");
    }
    if (args.Contains("--database-check", StringComparer.Ordinal)) return;
    await using var db = new DocuNestDbContext(new DbContextOptionsBuilder<DocuNestDbContext>().UseNpgsql(migration).AddInterceptors(new SearchPathGuardInterceptor()).Options);
    await db.Database.MigrateAsync();
    return;
}
await app.RunAsync();

public partial class Program;

public sealed class SafeExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            UseCaseException useCase => useCase.StatusCode,
            DbUpdateConcurrencyException => 409,
            BadHttpRequestException bad => bad.StatusCode,
            _ => 503
        };
        // Exception messages may contain SQL, credentials or document paths: never return/log them.
        context.Response.StatusCode = status;
        await Results.Problem(statusCode: status, title: exception is UseCaseException known ? known.Message : status == 409 ? "The resource changed. Reload before saving." : "The request could not be completed.",
            extensions: new Dictionary<string, object?> { ["code"] = exception is UseCaseException typed ? typed.Code : status == 409 ? "version_conflict" : "service_unavailable", ["correlationId"] = context.TraceIdentifier }).ExecuteAsync(context);
        return true;
    }
}

public sealed class DatabaseHealthCheck(IServiceScopeFactory scopes) : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
{
    public async Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var healthy = await scope.ServiceProvider.GetRequiredService<DocuNestDbContext>().Database.CanConnectAsync(cancellationToken);
            return healthy ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy() : Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Database unavailable");
        }
        catch { return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Database unavailable"); }
    }
}
