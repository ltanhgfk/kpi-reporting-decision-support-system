# Data Privacy & Public Release Policy

This document records exactly what was removed or changed from the original source before publication, and why. Nothing was removed "just in case" without a specific, stated reason.

## Removed — real credentials / internal infrastructure

| Item | Original location | Finding | Action |
|---|---|---|---|
| SQL Server credentials + internal server IP | `Web.config`, `connectionStrings` (both the active connection string and a commented-out legacy one) | Connection string(s) contained a live SQL Server `sa` account password and an internal network IP address | Both the active and the commented-out connection strings were replaced with placeholders (`YOUR_SERVER_ADDRESS`, `YOUR_DB_USER`, `YOUR_DB_PASSWORD`). **The real password and IP are not reproduced in this document or anywhere else in this repository.** If this credential has not already been rotated on the live database server, it should be changed immediately regardless of this repository's publication status. |

## Removed — real organizational data

| Item | Original location | Finding | Action |
|---|---|---|---|
| 6 real uploaded Excel reports | `Files/Uploads/*.xlsx` (timestamp+GUID filenames, Aug–Oct 2025) | Verified these are genuinely populated (one file inspected structurally contained 58 non-empty rows across 11 columns) and are produced by `BaoCaoController`'s real upload path, not fixtures | Deleted from the published repository; `.gitignore` prevents any future uploaded file (other than the sample template) from being committed |
| `Files/SampleBaoCao.xlsx`, `Files/Uploads/SampleBaoCao.xlsx` | — | Inspected structurally: header row only (`Địa bàn, Chỉ tiêu, Thực hiện (Lũy kế), Kế hoạch (lũy kế), Cùng kỳ (lũy kế), Đơn vị tính, Năm, Tháng, Ngày số liệu`), all data rows blank | Kept — confirmed to be a genuine blank template, not real data |

## Removed — developer machine / environment traces

| Item | Original location | Finding | Action |
|---|---|---|---|
| Local publish profile history | `Properties/PublishProfiles/` (entire folder: `FolderProfile.pubxml`, `FolderProfile1.pubxml`, and previously `FolderProfile.pubxml.user`, `FolderProfile1.pubxml.user`) | The `.user` files were removed in the first cleanup pass. A follow-up review found that the plain (non-`.user`) `.pubxml` files **also** contained the same absolute developer paths (`D:\WORKSPACE\WebDashBoard\Deployed\...`, `D:\WebPublic\Docker_...`) in their `<PublishUrl>` element — no credentials, but developer-machine-specific and not needed for a public/research release | Entire `Properties/PublishProfiles/` folder deleted |
| Visual Studio local project settings | `CT_Dashboard.csproj.user` | Contained `NameOfLastUsedPublishProfile` with an absolute developer path (`D:\WorkSpace\CT_Dashboard\...`); this file is IDE-local state, not part of the buildable project (confirmed: not referenced anywhere in `CT_Dashboard.csproj`) | Deleted |

## Removed — dead/duplicate source (development debris)

Numerous backup/duplicate controller, model, and view files accumulated during development (e.g., `DashboardController_old_.cs`, `Views/BaoCao/Temp/*`, `Views/ChiTieu_bk/*`). These were verified against `CT_Dashboard.csproj` to confirm which were actually compiled/included in the build versus purely orphaned files, then removed along with their corresponding `.csproj` entries. This does not change the application's runtime behavior — none of the removed files were reachable via routing in the active build. Full list of what was removed is in the release's final report (Phase 11, section D).

## Kept, with a disclosed note

| Item | Location | Note |
|---|---|---|
| Developer's personal name | `Global.asax.cs`: `ExcelPackage.License.SetNonCommercialPersonal("...")` | This is the EPPlus non-commercial license holder name, i.e., the author's own name attached to their own project. It is not a secret and is disclosed here rather than silently altered; the repository owner may replace it with a placeholder if they prefer not to have their name in the source. |

## Not independently verified

- The exact scope of what SQL Server user `sa` had access to on the original internal network is not knowable from source code alone, and is out of scope for a source-code-level privacy review.
