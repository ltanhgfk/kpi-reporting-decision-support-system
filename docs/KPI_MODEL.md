# KPI / Data Model

## Core entities (verified against `Models/*.cs`)

| Entity | Key fields | Role |
|---|---|---|
| `DON_VI` | ID, TEN_DON_VI | Organizational unit/department. **Flat list — no parent/child field.** |
| `DIA_BAN` | ID, TEN_DIA_BAN | Region/area. **Flat list — no parent/child field.** |
| `NGUOI_DUNG` | ID, VAI_TRO, DON_VI | User; `VAI_TRO` holds the role string (`Admin` / `User` / `Leader`) used by `AuthorizeRoleAttribute`. |
| `CHI_TIEU` | ID, TEN_CHI_TIEU, PARENTID | Indicator. `PARENTID` links a sub-indicator to an indicator group, forming a **two-level hierarchy**. This is the only hierarchical entity in the model. |
| `BAO_CAO` | ID, DON_VI, NAM, THANG, NGAY, TRANG_THAI_DUYET, LY_DO_TU_CHOI, NGUOI_TAO | A periodic report submitted by a unit, carrying its approval state. |
| `BAO_CAO_CHI_TIET` | BAO_CAO, CHI_TIEU, DIA_BAN, THUC_HIEN, KE_HOACH, CUNG_KY | Line item of a report: one indicator, one region, with actual/target/prior-period values stored side by side. |

## Why this model matters

The combination of `THUC_HIEN` (actual), `KE_HOACH` (target), and `CUNG_KY` (same period last year) on the **same row** is a deliberate design choice that supports three-way comparison directly in SQL/LINQ without needing to join across separate "actuals" and "targets" tables. This is what makes the dashboard's plan-vs-actual-vs-prior-year views possible without additional computation logic beyond simple arithmetic (e.g., percentage of target achieved).

## What is explicitly NOT a hierarchy

`DON_VI` (department) and `DIA_BAN` (region) are flat reference tables. There is no evidence in the schema or controller code of departmental hierarchy (e.g., sub-departments) or regional hierarchy (e.g., province → district). Only `CHI_TIEU` has a `PARENTID` relationship. Any documentation or presentation of this system should not imply a general "organizational hierarchy" beyond the indicator taxonomy.
