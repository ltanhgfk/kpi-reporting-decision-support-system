# Approval Workflow

## State machine (verified in `Controllers/BaoCaoController.cs`)

`BAO_CAO.TRANG_THAI_DUYET` holds one of the following string values:

```
Nhap          — draft, editable by its creator
TrinhLanhDao  — submitted, awaiting review by a Leader-role user
DaDuyet       — approved; only reports in this state are read by the Dashboard
TuChoi        — rejected; BAO_CAO.LY_DO_TU_CHOI stores the reviewer's rejection reason
```

## Role gating (verified via `[AuthorizeRole(...)]` attributes)

| Action | Required role(s) |
|---|---|
| Create/edit/import a report (`BaoCaoController`) | `Admin`, `User`, `Leader` (controller-level attribute) |
| Approve/reject a report (`DuyetBaoCao`, `DuyetBaoCaoForm`, `BaoCaoDaDuyet`) | `Leader` only (action-level attribute, verified at multiple action methods) |
| Manage indicators (`ChiTieuController`) | `Admin` |
| Manage departments/regions (`DonViController`, `DiaBanController`) | `Admin` |
| Manage users (`UserController`) | `Admin` |

## What happens on rejection

When a `Leader` rejects a report, the code records a reason (`LY_DO_TU_CHOI`) alongside the `TuChoi` status. This is real, structured rejection feedback rather than a bare status flag — the creator can see why a report was sent back.

## Known gap in this workflow

`DashboardController`, which reads approved report data, does **not** currently have an active `[AuthorizeRole]` attribute (it is present only as a comment). This means the workflow's role gating does not currently extend to viewing the dashboard itself. See `KNOWN_LIMITATIONS.md`.
