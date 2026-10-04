namespace DocuNest.Application.Abstractions;

public sealed record PasswordVerification(bool Valid, bool NeedsRehash);
public interface IPasswordHasher
{
    string Hash(string password);
    PasswordVerification Verify(string password, string encodedHash);
}

public interface ITokenSecretProvider
{
    string CreateToken();
    string Hash(string token);
    bool Verify(string token, string hash);
}

public sealed record AccessTokenSubject(Guid UserId, Guid SessionId, string SecurityStamp, bool MfaVerified);
public sealed record IssuedAccessToken(string Token, DateTimeOffset ExpiresAt);
public interface IAccessTokenIssuer
{
    IssuedAccessToken Issue(AccessTokenSubject subject);
}

public interface ISecretProtector
{
    string Protect(string plaintext, string resourceId);
    string Unprotect(string ciphertext, string resourceId);
}

public interface IMfaProvider
{
    string CreateSecret();
    string ProvisioningUri(string secret, string accountName);
    long? VerifyCounter(string secret, string code, DateTimeOffset now, long? lastAcceptedCounter);
}

public sealed record DownloadTicket(Guid SubjectId, Guid SessionId, Guid WorkspaceId, Guid DocumentId,
    Guid BlobId, string Action, DateTimeOffset ExpiresAt);
public interface IDownloadTicketProtector
{
    string Protect(DownloadTicket ticket);
    DownloadTicket? Unprotect(string token);
}

public sealed record CurrentUser(Guid UserId, Guid SessionId, string SecurityStamp, bool IsSystemAdmin,
    DateTimeOffset? MfaVerifiedAt);
public interface ICurrentUserAccessor { CurrentUser? Current { get; } }
