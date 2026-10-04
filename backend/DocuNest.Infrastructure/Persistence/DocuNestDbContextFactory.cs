using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DocuNest.Infrastructure.Persistence;

public sealed class DocuNestDbContextFactory : IDesignTimeDbContextFactory<DocuNestDbContext>
{
    public DocuNestDbContext CreateDbContext(string[] args)
    {
        // Design-time migrations require no server. Operators supply an explicit write connection.
        var connection = Environment.GetEnvironmentVariable("DOCUNEST_WRITE_CONNECTION")
            ?? "Host=localhost;Database=docunest;Username=docunest_migrator";
        return new DocuNestDbContext(new DbContextOptionsBuilder<DocuNestDbContext>()
            .UseNpgsql(connection)
            .AddInterceptors(new SearchPathGuardInterceptor())
            .Options);
    }
}
