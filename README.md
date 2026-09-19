# KPI Reporting & Decision Support System

A legacy ASP.NET MVC application for KPI-oriented reporting, plan-versus-actual analysis, data ingestion/validation, an approval workflow, and dashboard-based decision support.

This repository is published as a research/portfolio artifact documenting a real internal reporting system. It is not maintained as a commercial product, and it is not being modernized as part of this release — see [Known Limitations](#14-known-limitations) and [Historical Context](#15-historical-context).

---

## 1. Overview

Organizations that track periodic performance indicators (KPIs) across multiple departments and regions typically need to: collect numbers from several sources, validate them, compare them against targets and prior periods, route them through a review/approval step, and only then surface them to leadership on a dashboard. This system implements that full loop as a working ASP.NET MVC application.

## 2. System Purpose

Given monthly/yearly indicator data submitted by different organizational units, the system:
- ingests data via Excel upload, with structural and type validation before it reaches the database;
- represents indicators as a two-level hierarchy (indicator group → indicator), not a flat list;
- stores actual value, planned/target value, and same-period-last-year value together for every report line item;
- routes each report through an approval workflow (draft → submitted → approved/rejected, with a recorded rejection reason) before it is considered official;
- presents plan-vs-actual-vs-same-period comparisons on a dashboard, filterable by period, department, and region.

## 3. Main Components

| Component | File(s) | Responsibility |
|---|---|---|
| Dashboard | `Controllers/DashboardController.cs` | Reads approved report data and renders period-based KPI comparisons (Highcharts) |
| Indicator management | `Controllers/ChiTieuController.cs` | CRUD for indicators, including the group/sub-indicator hierarchy |
| Report & approval workflow | `Controllers/BaoCaoController.cs` | Report creation, Excel import, submission, approval/rejection |
| Data import & validation | `Controllers/ImportController.cs`, `Models/ExcelValidator.cs` | Validates uploaded Excel structure and per-row data types before writing to the database |
| Organizational reference data | `Controllers/DonViController.cs`, `Controllers/DiaBanController.cs` | CRUD for departments (units) and regions — flat lists, no hierarchy |
| User management | `Controllers/UserController.cs` | Admin-only user CRUD |
| Authentication | `Controllers/AccountController.cs` | Login, SHA-256 password hashing |
| Authorization | `Filters/AuthorizeRoleAttribute.cs` | Custom session-based role check (`Admin`, `User`, `Leader`) |

## 4. System Workflow

```
1. User logs in (AccountController) → session holds user + role
2. A report is created for a period (month/year) — manually, or via Excel upload
3. If uploaded: ExcelValidator checks the file structure and each row's data types
   before any row is written to BAO_CAO / BAO_CAO_CHI_TIET
4. Report status: Nhap (draft) → TrinhLanhDao (submitted) → DaDuyet (approved) / TuChoi (rejected, with a reason)
5. Only reports with status DaDuyet are read by the Dashboard
6. Dashboard queries report line items by department/region/period and renders
   actual vs. target vs. same-period-last-year comparisons
7. Report and indicator lists support keyword search and pagination
```

## 5. Architecture

```
Browser
   ↓
ASP.NET MVC Controllers (Dashboard, BaoCao, ChiTieu, DiaBan, DonVi, User, Account, Import, Admin)
   ↓
Business/validation logic (mostly inline in controllers; ExcelValidator is the one
extracted validation component)
   ↓
Entity Framework 6 (Database-First / EDMX) — direct DbContext access, no service layer
   ↓
SQL Server
   ↳ Excel files (.xlsx) are an input channel for data ingestion, not a parallel data store
   ↓
Razor Views + Highcharts (visualization) + PagedList (pagination)
```

There is no separate API layer, no service layer, and no repository pattern — controllers call Entity Framework directly. This is documented as a known limitation, not implied to be otherwise. See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

## 6. KPI / Reporting Concepts

- **Indicator hierarchy**: `CHI_TIEU` (indicator) has a `PARENTID` field, forming a two-level taxonomy (indicator group → indicator). This hierarchy exists **only** for indicators — `DIA_BAN` (region) and `DON_VI` (department) are flat reference lists with no parent/child structure.
- **Three-way comparison**: every report line item (`BAO_CAO_CHI_TIET`) stores `THUC_HIEN` (actual), `KE_HOACH` (target), and `CUNG_KY` (same period last year) together, enabling plan-vs-actual-vs-prior-year comparisons at the line-item level.
- **Time dimension**: reports carry `NAM` (year), `THANG` (month), and `NGAY` (date) fields.

Details in [`docs/KPI_MODEL.md`](docs/KPI_MODEL.md).

## 7. Data Ingestion & Validation

Reports can be created manually or imported from an Excel file. On upload (`BaoCaoController`), the file is:
1. Saved to `Files/Uploads/` with a timestamp+GUID filename (see [Data Privacy Policy](#16-public-release--data-policy) for why this folder is excluded from the public repo);
2. Validated by `Models/ExcelValidator.cs`, which checks the expected column headers and the data type of each cell before any row is committed to the database.

No feature exists to export current report data back out to Excel/PDF; the only file-download action returns a blank import template (`SampleBaoCao.xlsx`).

## 8. Approval Workflow

Reports move through an explicit multi-actor state machine, tracked in `BAO_CAO.TRANG_THAI_DUYET`:

```
Nhap (draft) → TrinhLanhDao (submitted to leader) → DaDuyet (approved)
                                                   → TuChoi (rejected, with LY_DO_TU_CHOI)
```

Approval/rejection actions are restricted to the `Leader` role via `[AuthorizeRole("Leader")]`. Details in [`docs/WORKFLOW.md`](docs/WORKFLOW.md).

## 9. Dashboard / Reporting

The dashboard (`DashboardController`) reads approved (`DaDuyet`) report line items for a selected period and renders comparisons (by department, by region, by indicator group) using Highcharts. Report and indicator lists elsewhere in the app support keyword search (`searchString`) and pagination (`PagedList`).

**Known gap**: the main `DashboardController` currently has its `[AuthorizeRole]` attribute commented out, meaning dashboard access is not enforced by an active authorization check in the code as it stands. See [`docs/KNOWN_LIMITATIONS.md`](docs/KNOWN_LIMITATIONS.md).

## 10. Repository Structure

```
.
├── README.md
├── PUBLIC_REPOSITORY_MANIFEST.md
├── .gitignore
├── docs/
│   ├── ARCHITECTURE.md
│   ├── DATA_FLOW.md
│   ├── KPI_MODEL.md
│   ├── WORKFLOW.md
│   ├── BUILD.md
│   ├── DEPENDENCIES.md
│   ├── KNOWN_LIMITATIONS.md
│   └── DATA_PRIVACY.md
└── CT_Dashboard/
    └── CT_Dashboard/
        ├── Controllers/
        ├── Models/
        ├── Views/
        ├── Filters/
        ├── App_Start/
        ├── Content/, Scripts/
        ├── Files/            (Uploads/ ignored except the sample template)
        ├── Web.config.example
        └── CT_Dashboard.csproj
```

## 11. Technology Stack

C# · ASP.NET MVC 5 · .NET Framework 4.7.2 · Entity Framework 6 (Database-First) · SQL Server · Highcharts · EPPlus / ClosedXML (Excel) · PagedList. Full list in [`docs/DEPENDENCIES.md`](docs/DEPENDENCIES.md).

## 12. Configuration

The real `Web.config` used in production contained a live database connection string with credentials and an internal server address; these have been replaced with placeholders. Copy `CT_Dashboard/CT_Dashboard/Web.config.example` to `Web.config` and fill in your own SQL Server connection details before running the project locally.

## 13. Running / Build Notes

This project targets **.NET Framework 4.7.2** and requires Visual Studio with the corresponding targeting pack, MSBuild, and NuGet package restore (`packages.config`). **Build was not verified as part of this release** — the environment used to prepare this repository does not have Windows/MSBuild/.NET Framework tooling available. See [`docs/BUILD.md`](docs/BUILD.md) for exact requirements and what remains unverified.

## 14. Known Limitations

- No service/repository layer — business logic lives in controllers.
- Session-based custom authorization rather than a standard identity framework.
- Password hashing uses unsalted SHA-256.
- The main dashboard controller's role-check attribute is currently commented out.
- No automated tests.
- No data export feature (only a blank template download).

Full list in [`docs/KNOWN_LIMITATIONS.md`](docs/KNOWN_LIMITATIONS.md).

## 15. Historical Context

This is a legacy internal tool, not a project built for public release from the outset. It is published here as-is, with sensitive data and credentials removed, to document real system-design and data-modeling work. No modernization (framework upgrade, added AI, added CI/CD, rewritten architecture) has been performed as part of this release — modernization ideas are listed only as future work, not implemented changes.

## 16. Public Release / Data Policy

- Real database credentials and an internal server address were removed from `Web.config` and replaced with placeholders.
- Six real, timestamped Excel files containing actual submitted report data were removed from `Files/Uploads/`; only the blank sample template (`SampleBaoCao.xlsx`) is kept.
- Local Visual Studio publish-profile files containing developer machine paths were removed.
- Duplicate/backup source files (uncompiled dead copies of controllers and views kept during development) were removed to keep the published source representative of what actually runs.

Full policy and rationale in [`docs/DATA_PRIVACY.md`](docs/DATA_PRIVACY.md) and [`PUBLIC_REPOSITORY_MANIFEST.md`](PUBLIC_REPOSITORY_MANIFEST.md).
