using DocuNest.Domain.Entities;

namespace DocuNest.Application.Abstractions;

/// <summary>One command owns a scoped unit of work. Repositories never commit independently.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

public interface IUserAccountRepository
{
    Task<UserAccount?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserAccount?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
    void Add(UserAccount user);
}

public interface IWorkspaceRepository
{
    Task<Workspace?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Workspace workspace);
}

public interface IDocumentRepository
{
    Task<Document?> GetAsync(Guid workspaceId, Guid id, CancellationToken cancellationToken = default);
    void Add(Document document);
}

/// <summary>Authoritative primary-database authorization reads; every action rechecks current membership and session state.</summary>
public interface IAuthorizationRepository
{
    Task<Membership?> GetMembershipAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    Task<DocumentGrant?> GetGrantAsync(Guid workspaceId, Guid documentId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountOwnersAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<RefreshSession?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
