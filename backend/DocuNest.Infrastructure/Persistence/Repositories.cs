using DocuNest.Application.Abstractions;
using DocuNest.Domain.Entities;
using DocuNest.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DocuNest.Infrastructure.Persistence;

public sealed class UserAccountRepository(DocuNestDbContext db) : IUserAccountRepository
{
    public Task<UserAccount?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<UserAccount?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    public void Add(UserAccount user) => db.Users.Add(user);
}

public sealed class WorkspaceRepository(DocuNestDbContext db) : IWorkspaceRepository
{
    public Task<Workspace?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Workspaces.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    public void Add(Workspace workspace) => db.Workspaces.Add(workspace);
}

public sealed class DocumentRepository(DocuNestDbContext db) : IDocumentRepository
{
    public Task<Document?> GetAsync(Guid workspaceId, Guid id, CancellationToken cancellationToken = default) =>
        db.Documents.SingleOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.Id == id, cancellationToken);
    public void Add(Document document) => db.Documents.Add(document);
}

public sealed class AuthorizationRepository(DocuNestDbContext db) : IAuthorizationRepository
{
    public Task<Membership?> GetMembershipAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default) =>
        db.Memberships.SingleOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.UserId == userId, cancellationToken);
    public Task<DocumentGrant?> GetGrantAsync(Guid workspaceId, Guid documentId, Guid userId, CancellationToken cancellationToken = default) =>
        db.DocumentGrants.SingleOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.DocumentId == documentId && x.UserId == userId, cancellationToken);
    public Task<int> CountOwnersAsync(Guid workspaceId, CancellationToken cancellationToken = default) =>
        db.Memberships.CountAsync(x => x.WorkspaceId == workspaceId && x.Role == WorkspaceRole.Owner, cancellationToken);
    public Task<RefreshSession?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        db.RefreshSessions.SingleOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
}

public sealed class UnitOfWork(DocuNestDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        new UnitOfWorkTransaction(await db.Database.BeginTransactionAsync(cancellationToken));

    private sealed class UnitOfWorkTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) => transaction.CommitAsync(cancellationToken);
        public Task RollbackAsync(CancellationToken cancellationToken = default) => transaction.RollbackAsync(cancellationToken);
        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
