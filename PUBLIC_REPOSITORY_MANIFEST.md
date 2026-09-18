# Public Repository Manifest

This repository is a sanitized public release of the CT_Dashboard software project.

## Included

- ASP.NET MVC / .NET Framework source code
- KPI/reporting and dashboard application components
- Excel ingestion and validation source
- Entity Framework model definitions
- Public sample Excel template
- NuGet package manifest (`packages.config`)

## Excluded

- Real or operational uploaded Excel files
- Credentials and active database connection strings
- Visual Studio user/session state
- Build output (`bin/`, `obj/`)
- NuGet package binaries (`packages/`)
- Publish-profile user history and local deployment paths
- Temporary, backup, and obsolete source variants

## Configuration

The public `Web.config` does not contain an active database credential. Configure a local connection string outside the public repository before running the application.
