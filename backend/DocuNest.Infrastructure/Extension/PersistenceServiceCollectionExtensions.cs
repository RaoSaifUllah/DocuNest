using DocuNest.Application.Abstractions;
using DocuNest.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DocuNest.Infrastructure.Extension;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddDocuNestPersistence(this IServiceCollection services, string writeConnection, string? readConnection = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(writeConnection);
        services.AddDbContext<DocuNestDbContext>(options => options.UseNpgsql(writeConnection).AddInterceptors(new SearchPathGuardInterceptor()));
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDocumentQueries>(_ => new DapperDocumentQueries(readConnection ?? writeConnection));
        return services;
    }
}
