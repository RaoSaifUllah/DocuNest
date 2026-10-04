using DocuNest.Domain.Entities;
using DocuNest.Domain.Enums;

namespace DocuNest.Domain.Policies;

public static class DocumentPermissions
{
    public static bool CanView(Guid userId, Membership? membership, Document document, DocumentGrant? grant)
    {
        if (!IsMember(userId, membership, document.WorkspaceId)) return false;
        return membership!.Role == WorkspaceRole.Owner
            || document.Visibility == DocumentVisibility.Workspace
            || document.UploaderId == userId
            || ValidGrant(userId, document, grant);
    }

    public static bool CanEdit(Guid userId, Membership? membership, Document document, DocumentGrant? grant) =>
        CanView(userId, membership, document, grant) && membership!.Role != WorkspaceRole.ReadOnly
        && (membership.Role == WorkspaceRole.Owner || document.Visibility == DocumentVisibility.Workspace
            || document.UploaderId == userId || ValidGrant(userId, document, grant) && grant!.CanEdit);

    // Deleting another member's workspace-wide document requires an explicit edit grant.
    public static bool CanTrash(Guid userId, Membership? membership, Document document, DocumentGrant? grant) =>
        CanView(userId, membership, document, grant) && membership!.Role != WorkspaceRole.ReadOnly
        && (membership.Role == WorkspaceRole.Owner || ValidGrant(userId, document, grant) && grant!.CanEdit);

    public static bool CanShare(Guid userId, Membership? membership, Document document, DocumentGrant? grant) =>
        CanView(userId, membership, document, grant) && document.TrashedAt is null
        && membership!.Role != WorkspaceRole.ReadOnly
        && (membership.Role == WorkspaceRole.Owner || ValidGrant(userId, document, grant) && grant!.CanShare);

    public static bool CanExport(Guid userId, Membership? membership, Document document, DocumentGrant? grant) =>
        CanView(userId, membership, document, grant) && membership!.Role != WorkspaceRole.ReadOnly
        && (membership.Role == WorkspaceRole.Owner || ValidGrant(userId, document, grant) && grant!.CanExport);

    public static bool CanCreatePersonalReminder(Guid userId, Membership? membership, Document document, DocumentGrant? grant) =>
        document.TrashedAt is null && CanView(userId, membership, document, grant);

    public static bool CanDownload(Guid userId, Membership? membership, Document document, DocumentGrant? grant, Blob blob) =>
        document.TrashedAt is null && document.PurgeRequestedAt is null
        && blob.WorkspaceId == document.WorkspaceId && blob.DocumentId == document.Id
        && blob.ScanStatus == ScanStatus.Clean && blob.DeletedAt is null && blob.PromotedAt is not null
        && (blob.Kind == BlobKind.Original || blob.Generation == document.ProcessingGeneration)
        && CanView(userId, membership, document, grant);

    private static bool IsMember(Guid userId, Membership? membership, Guid workspaceId) =>
        membership is not null && membership.UserId == userId && membership.WorkspaceId == workspaceId;

    private static bool ValidGrant(Guid userId, Document document, DocumentGrant? grant) =>
        grant is not null && grant.UserId == userId && grant.WorkspaceId == document.WorkspaceId && grant.DocumentId == document.Id;
}

public static class WorkspacePermissions
{
    public static bool CanUpload(Membership? membership) => membership?.Role is WorkspaceRole.Owner or WorkspaceRole.Member;
    public static bool CanManage(Membership? membership) => membership?.Role == WorkspaceRole.Owner;
    public static bool CanChangeRole(WorkspaceRole currentRole, WorkspaceRole? replacement, int ownerCount) =>
        currentRole != WorkspaceRole.Owner || replacement == WorkspaceRole.Owner || ownerCount > 1;
}
