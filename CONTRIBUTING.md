# Contributing

Read document/01–04 and Implementation-Status.md. Use .NET 10 SDK pinned by global.json and Node 24 LTS. Run locked restores, builds, unit/integration/browser checks before review. PostgreSQL integration checks use disposable schemas and synthetic records; never run against production. Keep secrets in ignored configuration, documents outside the web root, and processing tools isolated. Record architecture changes as ADRs. Do not bypass quarantine to make tests pass.

The owner authorized https://github.com/RaoSaifUllah/DocuNest and selected AGPL-3.0-only on 2026-10-04. Progress pushes are authorized; external deployment still requires explicit authorization.
