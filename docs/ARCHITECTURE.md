# Architecture

## Layer diagram (verified against source)

```
Browser
   ↓
ASP.NET MVC Controllers
   Controllers/DashboardController.cs
   Controllers/BaoCaoController.cs
   Controllers/ChiTieuController.cs
   Controllers/ImportController.cs
   Controllers/DonViController.cs
   Controllers/DiaBanController.cs
   Controllers/UserController.cs
   Controllers/AccountController.cs
   Controllers/AdminController.cs
   ↓
Business / validation logic
   - Mostly written directly inside controller actions (no dedicated service layer).
   - The one extracted exception: Models/ExcelValidator.cs, which validates uploaded
     Excel structure and per-row types independently of any controller.
   ↓
Entity Framework 6 (Database-First, EDMX)
   - Models/CTDashboardModel.cs / .Context.cs / .Designer.cs (auto-generated)
   - Controllers instantiate/use the EF context directly.
   ↓
SQL Server
   - Connection configured in Web.config (CT_DASHBOARDEntities).
   ↓
Views (Razor .cshtml) + Highcharts (charts) + PagedList (pagination)
```

## What this architecture is — and is not

This is a **two-tier MVC application with no service layer**: controllers depend directly on the Entity Framework `DbContext`. There is:
- no REST/Web API layer,
- no repository or unit-of-work abstraction,
- no message queue or background job system,
- no microservice boundary of any kind.

This is stated explicitly so the architecture is not mistaken for something more decoupled than it is. The most direct evidence: `BaoCaoController.cs` is 882 lines and contains report CRUD, Excel import, and approval-workflow logic together in one controller class.

## Authorization architecture

`Filters/AuthorizeRoleAttribute.cs` is a custom `AuthorizeAttribute` subclass that reads the current role from `Session["User"]` and compares it against a list of allowed roles passed to the attribute (e.g. `[AuthorizeRole("Admin", "User", "Leader")]`). This is **not** ASP.NET Identity or Forms Authentication — it is a hand-written session check. It is applied to `BaoCaoController`, `ChiTieuController`, `DiaBanController`, `DonViController`, and `UserController`. It is **not currently active** on `DashboardController` (the attribute is present in a comment but not applied).

## Data ingestion path

Excel files are an **input channel**, not a parallel data store: `ImportController` and `BaoCaoController` read uploaded `.xlsx` files (via EPPlus/ClosedXML), validate them with `ExcelValidator`, and write validated rows into the SQL Server database. The uploaded file itself is also retained on disk under `Files/Uploads/` for reference (see `docs/DATA_PRIVACY.md` for why real uploaded files were removed from this public release).
