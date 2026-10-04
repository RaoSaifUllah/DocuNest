# Third-party components

Application code is AGPL-3.0-only. Dependencies retain their own licenses. This inventory is a starting review; generated production image SBOM/license attribution is a release requirement.

| Component | Purpose | License / evidence |
|---|---|---|
| .NET / ASP.NET Core / EF Core | Runtime, HTTP, writes | MIT; official Microsoft packages |
| Npgsql / provider | PostgreSQL transport | PostgreSQL License |
| Dapper | Parameterized SELECT projections | Apache-2.0 |
| Konscious.Security.Cryptography.Argon2 | Argon2id password hashing | MIT; NuGet 1.3.1 dependency manifest |
| React / Vite / TypeScript / Tailwind / TanStack Query | Browser UI | MIT |
| PostgreSQL | Durable records/jobs | PostgreSQL License |
| Tesseract / language data | Local OCR | Apache-2.0; official project and per-pack attribution |
| Leptonica | Tesseract image decoding | BSD-2-Clause; additional codec notices required |
| Poppler utilities | PDF inspect/raster/text | GPL-2.0-or-later; packaged copyright covers bundled/transitive components |
| ClamAV | Local malware scanning | GPL-2.0; maintain current signatures |

Poppler components may depend on FreeType, Fontconfig, Cairo, OpenJPEG, JPEG, PNG, TIFF, zlib, libstdc++ and other packages. Tesseract depends on Leptonica and image codecs. Inspect the final built image dependency graph, retain `/usr/share/doc/*/copyright` and offer corresponding source where required. Rendering uses local native programs in the isolated processor; it is not a linked browser PDF viewer. No cloud OCR/scanner/analytics is selected.
