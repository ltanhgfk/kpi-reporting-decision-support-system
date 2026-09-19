# Public Repository Manifest

## Repository purpose

This repository documents a legacy ASP.NET MVC KPI reporting and decision-support system as a research/portfolio artifact. It is published to demonstrate real system-design, data-modeling, and workflow-engineering work — not as an actively maintained product, and not with any claim of AI/ML capability (none exists in the source).

## Included source

- Full ASP.NET MVC 5 application source under `CT_Dashboard/CT_Dashboard/`: Controllers, Models, Views, Filters, App_Start, Content, Scripts, Properties.
- `CT_Dashboard.csproj`, `packages.config` — build/dependency manifests.
- `Web.config.example` — sanitized configuration template (real credentials removed).
- One blank sample Excel template (`Files/SampleBaoCao.xlsx` and `Files/Uploads/SampleBaoCao.xlsx`), structurally verified to contain no real data.
- `docs/` — architecture, data flow, KPI model, workflow, build notes, dependency list, known limitations, and data-privacy documentation, each written directly from source-code evidence.

## Excluded private/runtime data

- The real database connection string (SQL Server credentials + internal IP address), removed from `Web.config`.
- Six real, timestamped Excel files containing actual submitted organizational report data, removed from `Files/Uploads/`.
- The entire `Properties/PublishProfiles/` folder (both the `.pubxml` and `.pubxml.user` files contained developer machine paths) and `CT_Dashboard.csproj.user` (IDE-local settings, also containing a developer machine path) — removed in full; neither is referenced by `CT_Dashboard.csproj`, so removal does not affect the buildable project.
- Build artifacts and IDE metadata (`bin/`, `obj/`, `.vs/`) and restorable NuGet packages (`packages/`) — excluded via `.gitignore`, not because they are sensitive, but because they are regenerable and not source.
- Duplicate/backup development files (old controller/view copies) that were never part of the compiled, routable application.

## Configuration policy

The application requires its own `Web.config` (copied from `Web.config.example`) with a real SQL Server connection string supplied locally. No working database connection is provided or implied by this repository.

## Known limitations

See [`docs/KNOWN_LIMITATIONS.md`](docs/KNOWN_LIMITATIONS.md) for the full list, including an authorization gap on the dashboard controller, unsalted password hashing, and the absence of a service/repository layer or automated tests.

## Dependency / build limitations

This project targets .NET Framework 4.7.2 and was **not built or run** as part of preparing this release, because the preparation environment had no Windows/MSBuild/.NET Framework tooling available. See [`docs/BUILD.md`](docs/BUILD.md) for exact requirements and what remains unverified. No code was altered to hide or work around a build failure — none was observed, because no build was attempted.

## Change policy for this release

Only the following categories of change were made to the original source:
1. Removal of real secrets/credentials and real organizational data (see `docs/DATA_PRIVACY.md` for the itemized list).
2. Removal of dead/duplicate files that were not part of the compiled application, plus the corresponding `.csproj` entries.
3. Addition of documentation (`README.md`, `docs/*.md`, this manifest) and a `.gitignore`.

No application behavior, business logic, or architecture was changed, refactored, or modernized as part of this release.
