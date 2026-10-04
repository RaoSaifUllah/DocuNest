# ADR 0004 — Explicit schema selection and development direct endpoint

Accepted 2026-10-04 after a real integration test caught ignored startup search-path state. Do not assume a connection string selected a schema. The supplied development endpoint is transaction-pooled; use the same database/credentials through its direct endpoint for EF writes and migrations. No database roles are created.

On opening an EF connection explicitly select the configured allowlisted schema and assert current_schema. Refuse custom-schema EF connections using the Neon pooler endpoint. Dapper uses an explicit read-only transaction and transaction-local schema selection/verification. Tests create generated disposable schemas and verify them before migrations or truncation. Migration startup asserts the chosen schema before applying changes.

Initial synthetic checks touched public before this assertion existed. Inventory and remove only objects created by the authored initial migration when all associated data is known synthetic; preserve unknown objects/data. Never drop the public schema or owner database.

Operational consequence: migrations require a direct/session-state-capable database connection. Connection pooling inside Npgsql remains supported. See [Neon connection pooling](https://neon.com/docs/connect/connection-pooling). This ADR records observed behavior and the defensive implementation; it does not claim a universal database-pooler guarantee.
