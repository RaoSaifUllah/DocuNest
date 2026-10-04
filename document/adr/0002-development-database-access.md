# ADR 0002 — Owner-selected application authorization and shared connection

Accepted 2026-10-04 following explicit owner instruction. Use the provided development connection for EF writes, migrations and Dapper reads. Do not create database access roles. This supersedes the separate SELECT-only role decision in ADR 0001 and planning document 02.

Application authorization checks current membership/document grants for every command, query and job. Dapper exposes parameterized SELECT projections with embedded workspace/visibility predicates and does not expose auth/share secrets. Read-only transactions defend against accidental writes where supported. The shared credential itself can write: application controls do not supply database-level least privilege. Tests must document this limitation instead of claiming separate-role enforcement.

Use synthetic data in development and disposable schemas for integration checks; do not delete the user's database. Credentials remain in ignored .env. No remote/push/publication is configured.
