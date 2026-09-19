# Data Flow

## End-to-end flow (verified against source)

```
1. Authentication
   AccountController.Login()
   → password hashed with SHA-256, compared against NGUOI_DUNG.PASSWORD
   → on success, user + role stored in Session

2. Report creation
   Option A — manual entry: user fills report line items directly.
   Option B — Excel upload (BaoCaoController.cs, ~line 326):
     a. File saved to Files/Uploads/{yyyyMMdd_HHmmss}_{GUID}_{originalFileName}.xlsx
     b. Models/ExcelValidator.cs checks:
        - the file has the expected column headers
        - each row's cells match the expected data type
     c. Only rows that pass validation are written to BAO_CAO / BAO_CAO_CHI_TIET

3. Approval workflow (see WORKFLOW.md for full detail)
   BAO_CAO.TRANG_THAI_DUYET: Nhap → TrinhLanhDao → DaDuyet / TuChoi

4. Dashboard read path
   DashboardController queries BAO_CAO_CHI_TIET joined with BAO_CAO
   filtered to TRANG_THAI_DUYET = "DaDuyet" and the selected period (NAM/THANG),
   grouped by DON_VI / DIA_BAN / CHI_TIEU, and passed to the view for
   Highcharts rendering.

5. Search / filter
   BaoCaoController.Index(string searchString, int? page) and
   ChiTieuController.Index(...) apply a LINQ .Where(...Contains(searchString)...)
   filter and return baocaos.ToPagedList(pageNumber, pageSize).
```

## Data that is NOT part of the flow

- No export of live report data to Excel/PDF exists. The only file-download action (`BaoCaoController`, ~line 880) returns the static blank import template `SampleBaoCao.xlsx`, not current database content.
- No background/scheduled job processes reports; all state transitions are triggered by a user action in a request.
