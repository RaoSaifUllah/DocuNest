# ADR 0001 — Supported .NET modular application and worker

Accepted 2026-10-04. Domain depends on no framework, Application on Domain, Infrastructure on Application, API/Worker compose them. EF aggregate repositories and one Unit of Work write commands; parameterized permission-scoped Dapper projections use a SELECT-only database identity. PostgreSQL owns durable jobs.

Use SDK 10.0.401 and runtime/package patch 10.0.12. Microsoft [support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core), checked 2026-10-04, identifies .NET 10 as active LTS through November 2028. .NET 11 is prerelease and is excluded. Node 24 is the frontend LTS major; pin actual compatible package versions/lockfiles.

Development database is the owner-authorized Neon PostgreSQL service with synthetic data. Original/derived document storage and processing stay local. This changes only development database hosting, not the self-hosted PostgreSQL production architecture.
