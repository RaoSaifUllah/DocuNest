using DocuNest.Domain.Enums;

namespace DocuNest.Application.Abstractions;

public interface IClock { DateTimeOffset UtcNow { get; } }

public sealed record StoredFile(string StorageKey, long SizeBytes, string Sha256);
public sealed record StoredFileInfo(string StorageKey, long SizeBytes, DateTimeOffset LastModifiedAt);

/// <summary>Keys are opaque generated identifiers; user filenames are never physical paths.</summary>
public interface IPrivateStorage
{
    Task<StoredFile> WriteStagingAsync(string key, Stream source, long maximumBytes, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task PromoteAsync(string sourceKey, string targetKey, string expectedSha256, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    IAsyncEnumerable<StoredFileInfo> EnumerateAsync(string prefix, CancellationToken cancellationToken = default);
}

public sealed record FileInspection(string ContentType, int PageCount, long MaximumPagePixels, bool Encrypted);
public sealed record ScanResult(ScanStatus Status, string? ErrorCode);
public interface IMalwareScanner
{
    Task<ScanResult> ScanAsync(string storageKey, CancellationToken cancellationToken = default);
}

public sealed record SandboxLimits(int MaximumPages = 200, long MaximumPixels = 40_000_000,
    int MaximumTextBytes = 4_000_000, int TimeoutSeconds = 120, int MemoryMiB = 512, int CpuSeconds = 90,
    long MaximumOutputBytes = 100L * 1024 * 1024);
public sealed record ProcessedPage(int PageNumber, string PreviewStorageKey, string ThumbnailStorageKey,
    string Text, string TextSource, string Language, double? Confidence);
public sealed record ProcessingOutput(int PageCount, IReadOnlyList<ProcessedPage> Pages);

/// <summary>Production adapter must run with no network and hard process-tree/resource limits.</summary>
public interface IDocumentProcessor
{
    Task<FileInspection> InspectAsync(string storageKey, SandboxLimits limits, CancellationToken cancellationToken = default);
    Task<ProcessingOutput> ProcessAsync(string storageKey, long generation, IReadOnlyList<string> languages,
        SandboxLimits limits, CancellationToken cancellationToken = default);
}

public sealed record NotificationEnvelope(Guid UserId, Guid WorkspaceId, Guid DocumentId, Guid ReminderId,
    long Generation, NotificationChannel Channel, string Template, IReadOnlyDictionary<string, string> Parameters);
public interface INotificationSender
{
    Task SendAsync(NotificationEnvelope notification, CancellationToken cancellationToken = default);
}

public sealed record ClaimedJob(Guid Id, Guid WorkspaceId, Guid? DocumentId, ProcessingStep Step,
    long Generation, long Fence, string LeaseOwner, string PayloadJson);
public interface IDurableJobQueue
{
    Task<ClaimedJob?> ClaimAsync(string leaseOwner, TimeSpan leaseDuration, CancellationToken cancellationToken = default);
    Task<bool> HeartbeatAsync(ClaimedJob claim, TimeSpan leaseDuration, CancellationToken cancellationToken = default);
    Task<bool> CompleteAsync(ClaimedJob claim, CancellationToken cancellationToken = default);
    Task<bool> FailAsync(ClaimedJob claim, string errorCode, bool transient, CancellationToken cancellationToken = default);
}
