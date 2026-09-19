# Build Notes

## Target framework (verified in `CT_Dashboard.csproj`)

```
<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
```

ASP.NET MVC 5 (`Microsoft.AspNet.Mvc` 5.2.9), Entity Framework 6.5.1, EPPlus 8.0.1, ClosedXML 0.104.2 (see `docs/DEPENDENCIES.md` for the full list from `packages.config`).

## Requirements to build (not installed in the environment used to prepare this release)

- Windows with Visual Studio (2019 or later recommended) **or** the .NET Framework 4.7.2 Developer Pack + MSBuild + NuGet CLI.
- SQL Server (any edition that supports the connection string format in `Web.config.example`) reachable from the build/run machine.
- NuGet package restore from `packages.config` (33 packages — see `docs/DEPENDENCIES.md`).

## Build status: NOT VERIFIED

This repository was prepared and sanitized in a Linux container with no MSBuild, no .NET Framework runtime, and no Visual Studio available. **No build or run attempt was made, and none is claimed.** Specifically:

- `msbuild` / `dotnet build` was not run against this project.
- NuGet restore was not attempted.
- The application was not started or smoke-tested.

Anyone building this project should expect to:
1. Restore NuGet packages (`packages/` was intentionally excluded from the repository — see `.gitignore`).
2. Copy `CT_Dashboard/CT_Dashboard/Web.config.example` to `Web.config` and provide a real SQL Server connection string.
3. Apply the database schema (no `.sql` schema dump is included in this release — the schema exists only as the EF Database-First model in `Models/CTDashboardModel.*`; extracting a standalone `.sql` script from this model has not been done as part of this release).

## Pre-existing, harmless path-casing quirks (not introduced by this cleanup)

The `.csproj` references a few files using a filename casing or URL-encoding that differs slightly from the actual file on disk (e.g., `Views\DonVi\Create.cshtml` vs. the file's actual name, and `%40`-encoded `@` in a couple of vendored JS filenames). These resolve correctly on Windows' case-insensitive filesystem and were confirmed present in the original, unmodified project before this release's cleanup — they are documented here rather than "fixed silently," per this release's policy of not altering application behavior.
