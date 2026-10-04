using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace DocuNest.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuration_secrets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ciphertext = table.Column<string>(type: "text", nullable: false),
                    key_version = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_configuration_secrets_id", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    display_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    security_stamp = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    is_system_admin = table.Column<bool>(type: "boolean", nullable: false),
                    email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    disabled = table.Column<bool>(type: "boolean", nullable: false),
                    encrypted_mfa_secret = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    mfa_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    last_mfa_counter = table.Column<long>(type: "bigint", nullable: true),
                    email_notifications_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    time_zone_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    locked_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failed_login_count = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users_id", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workspaces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    storage_quota_bytes = table.Column<long>(type: "bigint", nullable: false),
                    used_storage_bytes = table.Column<long>(type: "bigint", nullable: false),
                    reserved_storage_bytes = table.Column<long>(type: "bigint", nullable: false),
                    retention_days = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workspaces_id", x => x.id);
                    table.CheckConstraint("ck_workspace_quota", "storage_quota_bytes > 0 AND used_storage_bytes >= 0 AND reserved_storage_bytes >= 0 AND used_storage_bytes + reserved_storage_bytes <= storage_quota_bytes");
                    table.CheckConstraint("ck_workspace_retention", "retention_days BETWEEN 1 AND 3650");
                });

            migrationBuilder.CreateTable(
                name: "credential_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    token_hash = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    consumed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_credential_tokens_id", x => x.id);
                    table.ForeignKey(
                        name: "fk_credential_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mfa_recovery_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code_hash = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    consumed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mfa_recovery_codes_id", x => x.id);
                    table.ForeignKey(
                        name: "fk_mfa_recovery_codes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    csrf_hash = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    security_stamp = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    idle_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    absolute_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    consumed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    replaced_by_session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revocation_reason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    mfa_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    client_label = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_sessions_id", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_sessions_refresh_sessions_replaced_by_session_id",
                        column: x => x.replaced_by_session_id,
                        principalTable: "refresh_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_refresh_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    resource_type = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    outcome = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_events_id", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_events_users_actor_id",
                        column: x => x.actor_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_audit_events_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    color = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories_id", x => x.id);
                    table.UniqueConstraint("ak_categories_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.ForeignKey(
                        name: "fk_categories_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "export_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope = table.Column<int>(type: "integer", nullable: false),
                    document_ids_json = table.Column<string>(type: "jsonb", nullable: false),
                    include_trash = table.Column<bool>(type: "boolean", nullable: false),
                    manifest_version = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    artifact_storage_key = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    artifact_sha256 = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    artifact_bytes = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failure_code = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_export_jobs_id", x => x.id);
                    table.UniqueConstraint("ak_export_jobs_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.ForeignKey(
                        name: "fk_export_jobs_users_requester_id",
                        column: x => x.requester_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_export_jobs_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "import_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    manifest_version = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    total_files = table.Column<int>(type: "integer", nullable: false),
                    total_bytes = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    failure_code = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_import_jobs_id", x => x.id);
                    table.UniqueConstraint("ak_import_jobs_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.ForeignKey(
                        name: "fk_import_jobs_users_requester_id",
                        column: x => x.requester_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_import_jobs_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inviter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invitations_id", x => x.id);
                    table.UniqueConstraint("ak_invitations_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.ForeignKey(
                        name: "fk_invitations_users_inviter_id",
                        column: x => x.inviter_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_invitations_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "memberships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_memberships_id", x => x.id);
                    table.UniqueConstraint("ak_memberships_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.UniqueConstraint("ak_memberships_workspace_id_user_id", x => new { x.workspace_id, x.user_id });
                    table.CheckConstraint("ck_membership_role", "role BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "fk_memberships_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_memberships_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    color = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags_id", x => x.id);
                    table.UniqueConstraint("ak_tags_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.ForeignKey(
                        name: "fk_tags_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uploader_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    vendor = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    issue_date = table.Column<DateOnly>(type: "date", nullable: true),
                    document_date = table.Column<DateOnly>(type: "date", nullable: true),
                    expiration_date = table.Column<DateOnly>(type: "date", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    custom_metadata_json = table.Column<string>(type: "jsonb", nullable: false),
                    confirmed_fields_json = table.Column<string>(type: "jsonb", nullable: false),
                    logical_folder = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    visibility = table.Column<int>(type: "integer", nullable: false),
                    processing_status = table.Column<int>(type: "integer", nullable: false),
                    processing_generation = table.Column<long>(type: "bigint", nullable: false),
                    page_count = table.Column<int>(type: "integer", nullable: false),
                    failure_code = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    trashed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    purge_requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documents_id", x => x.id);
                    table.UniqueConstraint("ak_documents_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.CheckConstraint("ck_document_currency", "currency IS NULL OR currency ~ '^[A-Z]{3}$'");
                    table.CheckConstraint("ck_document_generation", "processing_generation > 0 AND page_count >= 0");
                    table.CheckConstraint("ck_document_visibility", "visibility BETWEEN 0 AND 1");
                    table.ForeignKey(
                        name: "fk_documents_categories_workspace_id_category_id",
                        columns: x => new { x.workspace_id, x.category_id },
                        principalTable: "categories",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_documents_users_uploader_id",
                        column: x => x.uploader_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_documents_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "blobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    pending_storage_key = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    original_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    scan_status = table.Column<int>(type: "integer", nullable: false),
                    generation = table.Column<long>(type: "bigint", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    promoted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blobs_id", x => x.id);
                    table.UniqueConstraint("ak_blobs_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.CheckConstraint("ck_blob_bounds", "size_bytes >= 0 AND generation > 0 AND (page_number IS NULL OR page_number > 0) AND sha256 ~ '^[a-f0-9]{64}$'");
                    table.ForeignKey(
                        name: "fk_blobs_documents_workspace_id_document_id",
                        columns: x => new { x.workspace_id, x.document_id },
                        principalTable: "documents",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_blobs_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "document_grants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    can_edit = table.Column<bool>(type: "boolean", nullable: false),
                    can_share = table.Column<bool>(type: "boolean", nullable: false),
                    can_export = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_grants_id", x => x.id);
                    table.UniqueConstraint("ak_document_grants_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.ForeignKey(
                        name: "fk_document_grants_documents_workspace_id_document_id",
                        columns: x => new { x.workspace_id, x.document_id },
                        principalTable: "documents",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_document_grants_memberships_workspace_id_user_id",
                        columns: x => new { x.workspace_id, x.user_id },
                        principalTable: "memberships",
                        principalColumns: new[] { "workspace_id", "user_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_document_grants_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "document_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_tags_id", x => x.id);
                    table.UniqueConstraint("ak_document_tags_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.ForeignKey(
                        name: "fk_document_tags_documents_workspace_id_document_id",
                        columns: x => new { x.workspace_id, x.document_id },
                        principalTable: "documents",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_document_tags_tags_workspace_id_tag_id",
                        columns: x => new { x.workspace_id, x.tag_id },
                        principalTable: "tags",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_document_tags_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "durable_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: true),
                    step = table.Column<int>(type: "integer", nullable: false),
                    generation = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    dedupe_key = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lease_owner = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    lease_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    fence = table.Column<long>(type: "bigint", nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    maximum_attempts = table.Column<int>(type: "integer", nullable: false),
                    last_error_code = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_durable_jobs_id", x => x.id);
                    table.UniqueConstraint("ak_durable_jobs_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.CheckConstraint("ck_job_attempts", "attempt_count >= 0 AND maximum_attempts BETWEEN 1 AND 100 AND fence >= 0");
                    table.ForeignKey(
                        name: "fk_durable_jobs_documents_workspace_id_document_id",
                        columns: x => new { x.workspace_id, x.document_id },
                        principalTable: "documents",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_durable_jobs_users_requester_id",
                        column: x => x.requester_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_durable_jobs_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "extraction_suggestions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    value = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    confidence = table.Column<double>(type: "double precision", nullable: false),
                    source = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    generation = table.Column<long>(type: "bigint", nullable: false),
                    accepted = table.Column<bool>(type: "boolean", nullable: false),
                    dismissed = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_extraction_suggestions_id", x => x.id);
                    table.UniqueConstraint("ak_extraction_suggestions_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.CheckConstraint("ck_suggestion_confidence", "confidence BETWEEN 0 AND 1 AND generation > 0");
                    table.ForeignKey(
                        name: "fk_extraction_suggestions_documents_workspace_id_document_id",
                        columns: x => new { x.workspace_id, x.document_id },
                        principalTable: "documents",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_extraction_suggestions_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "import_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    import_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    relative_path = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    failure_code = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_import_items_id", x => x.id);
                    table.UniqueConstraint("ak_import_items_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.ForeignKey(
                        name: "fk_import_items_documents_workspace_id_document_id",
                        columns: x => new { x.workspace_id, x.document_id },
                        principalTable: "documents",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_import_items_import_jobs_workspace_id_import_job_id",
                        columns: x => new { x.workspace_id, x.import_job_id },
                        principalTable: "import_jobs",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_import_items_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ocr_pages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: false),
                    generation = table.Column<long>(type: "bigint", nullable: false),
                    raw_text = table.Column<string>(type: "text", nullable: false),
                    normalized_text = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    confidence = table.Column<double>(type: "double precision", nullable: true),
                    source = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ocr_pages_id", x => x.id);
                    table.UniqueConstraint("ak_ocr_pages_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.CheckConstraint("ck_ocr_page_bounds", "page_number > 0 AND generation > 0 AND (confidence IS NULL OR confidence BETWEEN 0 AND 100)");
                    table.ForeignKey(
                        name: "fk_ocr_pages_documents_workspace_id_document_id",
                        columns: x => new { x.workspace_id, x.document_id },
                        principalTable: "documents",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ocr_pages_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reminders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    time_zone_id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    local_date = table.Column<DateOnly>(type: "date", nullable: false),
                    local_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    relative_days = table.Column<int>(type: "integer", nullable: true),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    generation = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    channel = table.Column<int>(type: "integer", nullable: false),
                    delivered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reminders_id", x => x.id);
                    table.UniqueConstraint("ak_reminders_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.ForeignKey(
                        name: "fk_reminders_documents_workspace_id_document_id",
                        columns: x => new { x.workspace_id, x.document_id },
                        principalTable: "documents",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_reminders_memberships_workspace_id_user_id",
                        columns: x => new { x.workspace_id, x.user_id },
                        principalTable: "memberships",
                        principalColumns: new[] { "workspace_id", "user_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_reminders_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "search_projections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    generation = table.Column<long>(type: "bigint", nullable: false),
                    search_text = table.Column<string>(type: "text", nullable: false),
                    language_config = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true, computedColumnSql: "to_tsvector('simple'::regconfig, coalesce(search_text, ''))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_search_projections_id", x => x.id);
                    table.UniqueConstraint("ak_search_projections_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.ForeignKey(
                        name: "fk_search_projections_documents_workspace_id_document_id",
                        columns: x => new { x.workspace_id, x.document_id },
                        principalTable: "documents",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_search_projections_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "share_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    allow_download = table.Column<bool>(type: "boolean", nullable: false),
                    maximum_downloads = table.Column<int>(type: "integer", nullable: true),
                    downloads_started = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_share_links_id", x => x.id);
                    table.UniqueConstraint("ak_share_links_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.CheckConstraint("ck_share_download_budget", "downloads_started >= 0 AND (maximum_downloads IS NULL OR (maximum_downloads > 0 AND downloads_started <= maximum_downloads)) AND expires_at > created_at");
                    table.ForeignKey(
                        name: "fk_share_links_documents_workspace_id_document_id",
                        columns: x => new { x.workspace_id, x.document_id },
                        principalTable: "documents",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_share_links_users_creator_id",
                        column: x => x.creator_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_share_links_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "upload_reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reserved_bytes = table.Column<long>(type: "bigint", nullable: false),
                    received_bytes = table.Column<long>(type: "bigint", nullable: false),
                    staging_key = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_upload_reservations_id", x => x.id);
                    table.UniqueConstraint("ak_upload_reservations_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.CheckConstraint("ck_reservation_bytes", "reserved_bytes >= 0 AND received_bytes >= 0 AND received_bytes <= reserved_bytes");
                    table.ForeignKey(
                        name: "fk_upload_reservations_documents_workspace_id_document_id",
                        columns: x => new { x.workspace_id, x.document_id },
                        principalTable: "documents",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_upload_reservations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_upload_reservations_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "job_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fence = table.Column<long>(type: "bigint", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error_code = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_attempts_id", x => x.id);
                    table.UniqueConstraint("ak_job_attempts_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.ForeignKey(
                        name: "fk_job_attempts_durable_jobs_workspace_id_job_id",
                        columns: x => new { x.workspace_id, x.job_id },
                        principalTable: "durable_jobs",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_job_attempts_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reminder_id = table.Column<Guid>(type: "uuid", nullable: false),
                    generation = table.Column<long>(type: "bigint", nullable: false),
                    channel = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications_id", x => x.id);
                    table.UniqueConstraint("ak_notifications_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.ForeignKey(
                        name: "fk_notifications_documents_workspace_id_document_id",
                        columns: x => new { x.workspace_id, x.document_id },
                        principalTable: "documents",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notifications_reminders_workspace_id_reminder_id",
                        columns: x => new { x.workspace_id, x.reminder_id },
                        principalTable: "reminders",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notifications_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "share_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    share_link_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    download_started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_share_sessions_id", x => x.id);
                    table.UniqueConstraint("ak_share_sessions_workspace_id_id", x => new { x.workspace_id, x.id });
                    table.ForeignKey(
                        name: "fk_share_sessions_documents_workspace_id_document_id",
                        columns: x => new { x.workspace_id, x.document_id },
                        principalTable: "documents",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_share_sessions_share_links_workspace_id_share_link_id",
                        columns: x => new { x.workspace_id, x.share_link_id },
                        principalTable: "share_links",
                        principalColumns: new[] { "workspace_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_share_sessions_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_actor_id",
                table: "audit_events",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_workspace_id_occurred_at",
                table: "audit_events",
                columns: new[] { "workspace_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_blobs_storage_key",
                table: "blobs",
                column: "storage_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_blobs_workspace_id",
                table: "blobs",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_blobs_workspace_id_document_id_kind_generation_page_number",
                table: "blobs",
                columns: new[] { "workspace_id", "document_id", "kind", "generation", "page_number" });

            migrationBuilder.CreateIndex(
                name: "ix_blobs_workspace_id_sha256_kind",
                table: "blobs",
                columns: new[] { "workspace_id", "sha256", "kind" });

            migrationBuilder.CreateIndex(
                name: "ix_categories_workspace_id",
                table: "categories",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_workspace_id_normalized_name",
                table: "categories",
                columns: new[] { "workspace_id", "normalized_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_configuration_secrets_name",
                table: "configuration_secrets",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_credential_tokens_expires_at",
                table: "credential_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_credential_tokens_token_hash",
                table: "credential_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_credential_tokens_user_id",
                table: "credential_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_document_grants_workspace_id",
                table: "document_grants",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_document_grants_workspace_id_document_id_user_id",
                table: "document_grants",
                columns: new[] { "workspace_id", "document_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_document_grants_workspace_id_user_id",
                table: "document_grants",
                columns: new[] { "workspace_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_document_tags_workspace_id",
                table: "document_tags",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_document_tags_workspace_id_document_id_tag_id",
                table: "document_tags",
                columns: new[] { "workspace_id", "document_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_document_tags_workspace_id_tag_id",
                table: "document_tags",
                columns: new[] { "workspace_id", "tag_id" });

            migrationBuilder.CreateIndex(
                name: "ix_documents_uploader_id",
                table: "documents",
                column: "uploader_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_workspace_id",
                table: "documents",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_workspace_id_category_id",
                table: "documents",
                columns: new[] { "workspace_id", "category_id" });

            migrationBuilder.CreateIndex(
                name: "ix_documents_workspace_id_expiration_date",
                table: "documents",
                columns: new[] { "workspace_id", "expiration_date" });

            migrationBuilder.CreateIndex(
                name: "ix_documents_workspace_id_trashed_at_created_at_id",
                table: "documents",
                columns: new[] { "workspace_id", "trashed_at", "created_at", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_durable_jobs_requester_id",
                table: "durable_jobs",
                column: "requester_id");

            migrationBuilder.CreateIndex(
                name: "ix_durable_jobs_status_due_at_lease_expires_at",
                table: "durable_jobs",
                columns: new[] { "status", "due_at", "lease_expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_durable_jobs_workspace_id",
                table: "durable_jobs",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_durable_jobs_workspace_id_dedupe_key",
                table: "durable_jobs",
                columns: new[] { "workspace_id", "dedupe_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_durable_jobs_workspace_id_document_id",
                table: "durable_jobs",
                columns: new[] { "workspace_id", "document_id" });

            migrationBuilder.CreateIndex(
                name: "ix_export_jobs_requester_id",
                table: "export_jobs",
                column: "requester_id");

            migrationBuilder.CreateIndex(
                name: "ix_export_jobs_workspace_id",
                table: "export_jobs",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_extraction_suggestions_workspace_id",
                table: "extraction_suggestions",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_extraction_suggestions_workspace_id_document_id_generation",
                table: "extraction_suggestions",
                columns: new[] { "workspace_id", "document_id", "generation" });

            migrationBuilder.CreateIndex(
                name: "ix_import_items_workspace_id",
                table: "import_items",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_import_items_workspace_id_document_id",
                table: "import_items",
                columns: new[] { "workspace_id", "document_id" });

            migrationBuilder.CreateIndex(
                name: "ix_import_items_workspace_id_import_job_id",
                table: "import_items",
                columns: new[] { "workspace_id", "import_job_id" });

            migrationBuilder.CreateIndex(
                name: "ix_import_jobs_requester_id",
                table: "import_jobs",
                column: "requester_id");

            migrationBuilder.CreateIndex(
                name: "ix_import_jobs_workspace_id",
                table: "import_jobs",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_inviter_id",
                table: "invitations",
                column: "inviter_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_token_hash",
                table: "invitations",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invitations_workspace_id",
                table: "invitations",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_workspace_id_normalized_email",
                table: "invitations",
                columns: new[] { "workspace_id", "normalized_email" });

            migrationBuilder.CreateIndex(
                name: "ix_job_attempts_workspace_id",
                table: "job_attempts",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_job_attempts_workspace_id_job_id_fence",
                table: "job_attempts",
                columns: new[] { "workspace_id", "job_id", "fence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_memberships_user_id_workspace_id",
                table: "memberships",
                columns: new[] { "user_id", "workspace_id" });

            migrationBuilder.CreateIndex(
                name: "ix_memberships_workspace_id",
                table: "memberships",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_mfa_recovery_codes_user_id_code_hash",
                table: "mfa_recovery_codes",
                columns: new[] { "user_id", "code_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_read_at",
                table: "notifications",
                columns: new[] { "user_id", "read_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_workspace_id",
                table: "notifications",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_workspace_id_document_id",
                table: "notifications",
                columns: new[] { "workspace_id", "document_id" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_workspace_id_reminder_id_generation_channel",
                table: "notifications",
                columns: new[] { "workspace_id", "reminder_id", "generation", "channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ocr_pages_workspace_id",
                table: "ocr_pages",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_ocr_pages_workspace_id_document_id_generation_page_number",
                table: "ocr_pages",
                columns: new[] { "workspace_id", "document_id", "generation", "page_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_sessions_absolute_expires_at",
                table: "refresh_sessions",
                column: "absolute_expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_sessions_replaced_by_session_id",
                table: "refresh_sessions",
                column: "replaced_by_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_sessions_token_hash",
                table: "refresh_sessions",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_sessions_user_id_family_id",
                table: "refresh_sessions",
                columns: new[] { "user_id", "family_id" });

            migrationBuilder.CreateIndex(
                name: "ix_reminders_status_due_at",
                table: "reminders",
                columns: new[] { "status", "due_at" });

            migrationBuilder.CreateIndex(
                name: "ix_reminders_workspace_id",
                table: "reminders",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_reminders_workspace_id_document_id",
                table: "reminders",
                columns: new[] { "workspace_id", "document_id" });

            migrationBuilder.CreateIndex(
                name: "ix_reminders_workspace_id_user_id",
                table: "reminders",
                columns: new[] { "workspace_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_search_projections_search_vector",
                table: "search_projections",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_search_projections_workspace_id",
                table: "search_projections",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_search_projections_workspace_id_document_id",
                table: "search_projections",
                columns: new[] { "workspace_id", "document_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_share_links_creator_id",
                table: "share_links",
                column: "creator_id");

            migrationBuilder.CreateIndex(
                name: "ix_share_links_token_hash",
                table: "share_links",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_share_links_workspace_id",
                table: "share_links",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_share_links_workspace_id_document_id",
                table: "share_links",
                columns: new[] { "workspace_id", "document_id" });

            migrationBuilder.CreateIndex(
                name: "ix_share_sessions_token_hash",
                table: "share_sessions",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_share_sessions_workspace_id",
                table: "share_sessions",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_share_sessions_workspace_id_document_id",
                table: "share_sessions",
                columns: new[] { "workspace_id", "document_id" });

            migrationBuilder.CreateIndex(
                name: "ix_share_sessions_workspace_id_share_link_id",
                table: "share_sessions",
                columns: new[] { "workspace_id", "share_link_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tags_workspace_id",
                table: "tags",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_workspace_id_normalized_name",
                table: "tags",
                columns: new[] { "workspace_id", "normalized_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_upload_reservations_status_expires_at",
                table: "upload_reservations",
                columns: new[] { "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_upload_reservations_user_id",
                table: "upload_reservations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_upload_reservations_workspace_id",
                table: "upload_reservations",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_upload_reservations_workspace_id_document_id",
                table: "upload_reservations",
                columns: new[] { "workspace_id", "document_id" });

            migrationBuilder.CreateIndex(
                name: "ix_users_normalized_email",
                table: "users",
                column: "normalized_email",
                unique: true);
            migrationBuilder.Sql(ReviewedReadViews.CreateSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ReviewedReadViews.DropSql);
            migrationBuilder.DropTable(
                name: "audit_events");

            migrationBuilder.DropTable(
                name: "blobs");

            migrationBuilder.DropTable(
                name: "configuration_secrets");

            migrationBuilder.DropTable(
                name: "credential_tokens");

            migrationBuilder.DropTable(
                name: "document_grants");

            migrationBuilder.DropTable(
                name: "document_tags");

            migrationBuilder.DropTable(
                name: "export_jobs");

            migrationBuilder.DropTable(
                name: "extraction_suggestions");

            migrationBuilder.DropTable(
                name: "import_items");

            migrationBuilder.DropTable(
                name: "invitations");

            migrationBuilder.DropTable(
                name: "job_attempts");

            migrationBuilder.DropTable(
                name: "mfa_recovery_codes");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "ocr_pages");

            migrationBuilder.DropTable(
                name: "refresh_sessions");

            migrationBuilder.DropTable(
                name: "search_projections");

            migrationBuilder.DropTable(
                name: "share_sessions");

            migrationBuilder.DropTable(
                name: "upload_reservations");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "import_jobs");

            migrationBuilder.DropTable(
                name: "durable_jobs");

            migrationBuilder.DropTable(
                name: "reminders");

            migrationBuilder.DropTable(
                name: "share_links");

            migrationBuilder.DropTable(
                name: "memberships");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "workspaces");
        }
    }
}
