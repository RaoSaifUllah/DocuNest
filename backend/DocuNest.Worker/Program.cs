using DocuNest.Infrastructure.Configuration;
using DocuNest.Infrastructure.Persistence;
using DocuNest.Infrastructure.Extension;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
EnvironmentFile.LoadDevelopment(builder.Environment.ContentRootPath, builder.Environment.IsDevelopment());
builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<RuntimeOptions>(builder.Configuration.GetSection("DocuNest"));
builder.Services.AddDocuNestPersistence(builder.Configuration.GetConnectionString("Write") ?? throw new InvalidOperationException("ConnectionStrings:Write is required."));
var app = builder.Build();
app.MapGet("/health/live", () => Results.Ok(new { status = "alive" }));
await app.RunAsync();
