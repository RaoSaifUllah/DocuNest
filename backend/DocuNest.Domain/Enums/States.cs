namespace DocuNest.Domain.Enums;

public enum WorkspaceRole { Owner, Member, ReadOnly }
public enum DocumentVisibility { Workspace, Restricted }
public enum DocumentProcessingStatus { Quarantined, Uploaded, Queued, Processing, Completed, Failed, Purging }
public enum BlobKind { Original, Preview, Thumbnail, Export }
public enum ScanStatus { Pending, Clean, Infected, Unavailable, Failed }
public enum CredentialTokenKind { EmailVerification, PasswordReset, Invitation, Bootstrap }
public enum JobStatus { Pending, Running, Completed, Failed, Cancelled }
public enum ProcessingStep { Scan, Promote, Inspect, Render, Extract, Ocr, Suggest, Index, Reminder, Notify, Purge, Export, Import, Reconcile }
public enum ReminderStatus { Scheduled, Delivered, Cancelled, Failed }
public enum ReminderKind { Custom, Expiration, Warranty, Renewal, Payment }
public enum NotificationChannel { InApp, Email }
public enum ReservationStatus { Open, Finalized, Expired }
public enum ImportItemStatus { Pending, Quarantined, Completed, Duplicate, Failed, Cancelled }
public enum ExportScope { Document, Selected, Workspace }
