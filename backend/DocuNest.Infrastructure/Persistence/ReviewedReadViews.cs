namespace DocuNest.Infrastructure.Persistence;

/// <summary>Reviewed projection columns. Authentication, sharing secrets and private storage keys are deliberately absent.</summary>
internal static class ReviewedReadViews
{
    internal const string CreateSql = """
        CREATE VIEW read_workspaces WITH (security_barrier = true) AS
          SELECT id, name, storage_quota_bytes, used_storage_bytes, reserved_storage_bytes, retention_days, created_at, version FROM workspaces;
        CREATE VIEW read_memberships WITH (security_barrier = true) AS
          SELECT id, workspace_id, user_id, role, created_at, version FROM memberships;
        CREATE VIEW read_documents WITH (security_barrier = true) AS
          SELECT id, workspace_id, uploader_id, title, description, vendor, amount, currency, issue_date, document_date,
            expiration_date, category_id, custom_metadata_json, confirmed_fields_json, logical_folder, visibility,
            processing_status, processing_generation, page_count, failure_code, trashed_at, purge_requested_at,
            created_at, updated_at, version FROM documents;
        CREATE VIEW read_document_grants WITH (security_barrier = true) AS
          SELECT id, workspace_id, document_id, user_id, can_edit, can_share, can_export, version FROM document_grants;
        CREATE VIEW read_blobs WITH (security_barrier = true) AS
          SELECT id, workspace_id, document_id, original_name, content_type, size_bytes, sha256, kind, scan_status,
            generation, page_number, created_at, promoted_at, deleted_at, version FROM blobs;
        CREATE VIEW read_ocr_pages WITH (security_barrier = true) AS
          SELECT id, workspace_id, document_id, page_number, generation, raw_text, normalized_text, language, confidence, source, version FROM ocr_pages;
        CREATE VIEW read_categories WITH (security_barrier = true) AS
          SELECT id, workspace_id, name, normalized_name, color, version FROM categories;
        CREATE VIEW read_tags WITH (security_barrier = true) AS
          SELECT id, workspace_id, name, normalized_name, color, version FROM tags;
        CREATE VIEW read_document_tags WITH (security_barrier = true) AS
          SELECT id, workspace_id, document_id, tag_id, version FROM document_tags;
        CREATE VIEW read_search_projections WITH (security_barrier = true) AS
          SELECT id, workspace_id, document_id, generation, search_text, language_config, search_vector, published_at, version FROM search_projections;
        CREATE VIEW read_reminders WITH (security_barrier = true) AS
          SELECT id, workspace_id, document_id, user_id, time_zone_id, local_date, local_time, relative_days, due_at,
            generation, status, kind, channel, delivered_at, version FROM reminders;
        CREATE VIEW read_notifications WITH (security_barrier = true) AS
          SELECT id, workspace_id, document_id, user_id, reminder_id, generation, channel, created_at, read_at, sent_at, version FROM notifications;
        CREATE VIEW read_audit_events WITH (security_barrier = true) AS
          SELECT id, workspace_id, actor_id, action, resource_type, resource_id, occurred_at, correlation_id, outcome, version FROM audit_events;
        """;

    internal const string DropSql = """
        DROP VIEW read_audit_events, read_notifications, read_reminders, read_search_projections,
          read_document_tags, read_tags, read_categories, read_ocr_pages, read_blobs,
          read_document_grants, read_documents, read_memberships, read_workspaces;
        """;
}
