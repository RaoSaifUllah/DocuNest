using DocuNest.Domain.Entities;
using DocuNest.Domain.Enums;

namespace DocuNest.Domain.Policies;

public static class ProcessingPolicy
{
    public static bool CanTransition(DocumentProcessingStatus current, DocumentProcessingStatus next) => (current, next) switch
    {
        (DocumentProcessingStatus.Quarantined, DocumentProcessingStatus.Uploaded) => true,
        (DocumentProcessingStatus.Uploaded, DocumentProcessingStatus.Queued) => true,
        (DocumentProcessingStatus.Queued, DocumentProcessingStatus.Processing) => true,
        (DocumentProcessingStatus.Processing, DocumentProcessingStatus.Completed) => true,
        (DocumentProcessingStatus.Processing, DocumentProcessingStatus.Failed) => true,
        (DocumentProcessingStatus.Quarantined, DocumentProcessingStatus.Failed) => true,
        (DocumentProcessingStatus.Uploaded, DocumentProcessingStatus.Failed) => true,
        (DocumentProcessingStatus.Queued, DocumentProcessingStatus.Failed) => true,
        (DocumentProcessingStatus.Completed, DocumentProcessingStatus.Queued) => true,
        (DocumentProcessingStatus.Failed, DocumentProcessingStatus.Queued) => true,
        (_, DocumentProcessingStatus.Purging) => current != DocumentProcessingStatus.Purging,
        _ => false
    };

    public static bool CanPublish(Document document, long generation) =>
        document.ProcessingGeneration == generation && document.TrashedAt is null && document.PurgeRequestedAt is null
        && document.ProcessingStatus == DocumentProcessingStatus.Processing;

    public static void InvalidateGeneration(Document document, DateTimeOffset now)
    {
        document.ProcessingGeneration = checked(document.ProcessingGeneration + 1);
        document.UpdatedAt = now;
    }

    public static bool LeaseIsCurrent(DurableJob job, string owner, long fence, DateTimeOffset now) =>
        job.Status == JobStatus.Running && job.LeaseOwner == owner && job.Fence == fence && job.LeaseExpiresAt > now;

    public static TimeSpan RetryDelay(int attempt) => TimeSpan.FromSeconds(Math.Min(900, Math.Pow(2, Math.Clamp(attempt, 0, 10)) * 5));
}

public static class QuotaPolicy
{
    public static bool CanReserve(Workspace workspace, long bytes) => bytes > 0 && workspace.UsedStorageBytes >= 0
        && workspace.ReservedStorageBytes >= 0 && workspace.StorageQuotaBytes >= workspace.UsedStorageBytes
        && bytes <= workspace.StorageQuotaBytes - workspace.UsedStorageBytes
        && workspace.ReservedStorageBytes <= workspace.StorageQuotaBytes - workspace.UsedStorageBytes - bytes;
}
