# DocuNest
Scan it. Store it. Find it later.
Privacy-focused self-hosted document vault for personal, family and small-business use.

Status: implementation in progress. See document/Implementation-Status.md for
executed checks, implemented milestones and unresolved release gates.
Repository: https://github.com/RaoSaifUllah/DocuNest
License: AGPL-3.0-only, selected by the owner on 2026-10-04. See LICENSE.

Read document/01-Requirements-and-Review.md, 02-Software-Design.md,
03-End-to-End-Development-Plan.md, 04-Verification-and-Standards.md and
05-Development-Chat-Prompt.md before implementation.

Frontend: React/TypeScript/Vite/Tailwind CSS responsive web/PWA.
backend: .NET 10 LTS API and worker, Domain/Application/Infrastructure layers,
EF Core repositories/Unit of Work writes and Dapper reads, PostgreSQL.
tests: browser journeys and sanitized OCR fixtures. deploy/scripts: containers,
migrations and operational tools. .github/workflows: independent project CI.

Mobile camera capture is in the source; responsive browser capture satisfies
the initial requirement. Native apps are explicitly outside the supplied MVP,
so no Mobile folder is created. Later native scope must use React Native,
TypeScript and NativeWind for Android/iOS only; iOS builds need macOS tooling.
PWA share-target behavior varies by platform; expose file-picker fallback.
Recheck compatible stable package versions at implementation.
