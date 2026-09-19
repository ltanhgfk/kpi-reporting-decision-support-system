# Known Limitations

Stated deliberately, based on direct code review — not hidden or downplayed.

## Architecture

- No service or repository layer: business logic lives directly in controller actions. `BaoCaoController.cs` alone is 882 lines, mixing CRUD, Excel import, and approval-workflow logic.
- No automated tests exist anywhere in the codebase.
- No API layer; the application is a single server-rendered MVC app.

## Security

- **Authorization gap**: `DashboardController` does not currently have an active `[AuthorizeRole]` attribute (only present as a comment), so dashboard access is not enforced by the codebase's own role-check mechanism as it stands.
- Passwords are hashed with **unsalted SHA-256** (`AccountController.cs`) — adequate to demonstrate the authentication flow, not adequate for a production system handling real credentials.
- Authorization is a custom session-based check (`AuthorizeRoleAttribute`), not a standard framework like ASP.NET Identity; session-based authorization can be more fragile (e.g., under session expiry/config issues) than token- or cookie-claims-based approaches.

## Data model

- Report structure and workflow state live in a Database-First EF model with no separate migrations history — schema changes were made directly against the database, and no standalone `.sql` schema script is included in this release (see `docs/BUILD.md`).
- Only indicators (`CHI_TIEU`) have a hierarchy (`PARENTID`); departments and regions are flat lists. Documentation and any external description of this system should not overstate this into a general organizational hierarchy.

## Functionality

- No data export feature: the only file-download action returns a static blank import template, not live report data.
- No automated notifications (e.g., email) on report submission/approval/rejection were found in the reviewed code.

## Pre-existing build quirks

A handful of `.csproj` file references use a filename casing or URL-encoding that differs slightly from the actual filename on disk (see `docs/BUILD.md`). These are harmless on Windows (case-insensitive filesystem) and were present before this release's cleanup — they were not introduced by, nor fixed by, this release.

## Not verified in this release

- The project was not built or run (no Windows/MSBuild environment was available during preparation — see `docs/BUILD.md`).
- The full SQL Server schema was not independently reconstructed/tested outside of the EF Database-First model already in the source.
