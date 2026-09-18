using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using Newtonsoft.Json;
using CT_Dashboard.Models;// Thay bằng namespace của bạn

namespace CT_Dashboard.Controllers
{
    public class ImportController : Controller
    {
        private readonly CT_DASHBOARDEntities _db = new CT_DASHBOARDEntities(); // Thay bằng DbContext của bạn

        // GET: Import
        public ActionResult Import()
        {
            return View();
        }

        // POST: PreviewImport - Xem trước dữ liệu
        [HttpPost]
        public ActionResult PreviewImport(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                ViewBag.Error = "Vui lòng chọn file Excel!";
                return View("Import");
            }

            try
            {
                //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage.License.SetNonCommercialPersonal("Luong The Anh");
                var chiTieuList = new List<CHI_TIEU>();
                var warnings = new List<string>();

                using (var package = new ExcelPackage(file.InputStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        ViewBag.Error = "File Excel không có sheet nào!";
                        return View("Import");
                    }

                    int rowCount = worksheet.Dimension?.Rows ?? 0;
                    if (rowCount < 2)
                    {
                        ViewBag.Error = "File Excel không có dữ liệu!";
                        return View("Import");
                    }

                    // Đọc dữ liệu từ dòng 2 (dòng 1 là header)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var maChiTieu = worksheet.Cells[row, 1].Value?.ToString()?.Trim();

                        // Bỏ qua dòng trống
                        if (string.IsNullOrWhiteSpace(maChiTieu))
                            continue;

                        var chiTieu = new CHI_TIEU
                        {
                            MA_CHI_TIEU = maChiTieu,
                            TEN_CHI_TIEU = worksheet.Cells[row, 2].Value?.ToString()?.Trim(),
                            TEN_DAY_DU = worksheet.Cells[row, 3].Value?.ToString()?.Trim(),
                            MA_MAU = worksheet.Cells[row, 4].Value?.ToString()?.Trim(),
                            PARENTID = worksheet.Cells[row, 5].Value?.ToString()?.Trim(),
                            DON_VI = worksheet.Cells[row, 6].Value?.ToString()?.Trim(),
                            DVT = worksheet.Cells[row, 7].Value?.ToString()?.Trim(),
                            TRANG_THAI = worksheet.Cells[row, 9].Value?.ToString()?.Trim()
                        };

                        // Xử lý VI_TRI
                        var viTriValue = worksheet.Cells[row, 8].Value;
                        if (viTriValue != null)
                        {
                            byte viTri;
                            if (byte.TryParse(viTriValue.ToString(), out viTri))
                            {
                                chiTieu.VI_TRI = viTri;
                            }
                        }

                        // Xử lý QUAN_TRONG
                        var quanTrongValue = worksheet.Cells[row, 10].Value?.ToString()?.Trim().ToLower();
                        if (!string.IsNullOrWhiteSpace(quanTrongValue))
                        {
                            if (quanTrongValue == "1" || quanTrongValue == "true" || quanTrongValue == "có" || quanTrongValue == "yes")
                            {
                                chiTieu.QUAN_TRONG = true;
                            }
                            else if (quanTrongValue == "0" || quanTrongValue == "false" || quanTrongValue == "không" || quanTrongValue == "no")
                            {
                                chiTieu.QUAN_TRONG = false;
                            }
                            else
                            {
                                chiTieu.QUAN_TRONG = null;
                            }
                        }

                        chiTieuList.Add(chiTieu);
                    }
                }

                // Kiểm tra dữ liệu
                ValidateData(chiTieuList, warnings);

                // Lưu vào ViewBag để hiển thị
                ViewBag.Warnings = warnings;
                ViewBag.JsonData = JsonConvert.SerializeObject(chiTieuList);

                return View("Import", chiTieuList);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi khi đọc file: {ex.Message}";
                return View("Import");
            }
        }

        // POST: SaveImport - Lưu dữ liệu vào CSDL
        [HttpPost]
        public ActionResult SaveImport(string jsonData)
        {
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                ViewBag.Error = "Không có dữ liệu để lưu!";
                return View("Import");
            }

            try
            {
                var chiTieuList = JsonConvert.DeserializeObject<List<CHI_TIEU>>(jsonData);

                if (chiTieuList == null || chiTieuList.Count == 0)
                {
                    ViewBag.Error = "Không có dữ liệu để lưu!";
                    return View("Import");
                }

                int successCount = 0;
                int errorCount = 0;
                var errors = new List<string>();

                foreach (var chiTieu in chiTieuList)
                {
                    try
                    {
                        // Kiểm tra xem mã chỉ tiêu đã tồn tại chưa
                        var existing = _db.CHI_TIEU.FirstOrDefault(x => x.MA_CHI_TIEU == chiTieu.MA_CHI_TIEU);

                        if (existing != null)
                        {
                            // Cập nhật nếu đã tồn tại
                            existing.TEN_CHI_TIEU = chiTieu.TEN_CHI_TIEU;
                            existing.TEN_DAY_DU = chiTieu.TEN_DAY_DU;
                            existing.MA_MAU = chiTieu.MA_MAU;
                            existing.PARENTID = chiTieu.PARENTID;
                            existing.DON_VI = chiTieu.DON_VI;
                            existing.DVT = chiTieu.DVT;
                            existing.VI_TRI = chiTieu.VI_TRI;
                            existing.TRANG_THAI = chiTieu.TRANG_THAI;
                            existing.QUAN_TRONG = chiTieu.QUAN_TRONG;
                        }
                        else
                        {
                            // Thêm mới nếu chưa tồn tại
                            _db.CHI_TIEU.Add(chiTieu);
                        }

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.Add($"Lỗi tại mã {chiTieu.MA_CHI_TIEU}: {ex.Message}");
                    }
                }

                // Lưu tất cả thay đổi
                _db.SaveChanges();

                if (errorCount > 0)
                {
                    ViewBag.Error = $"Đã lưu {successCount} bản ghi thành công, {errorCount} bản ghi lỗi. " + string.Join("; ", errors);
                }
                else
                {
                    ViewBag.Success = $"Đã lưu thành công {successCount} bản ghi vào CSDL!";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi khi lưu dữ liệu: {ex.Message}";
                return View("Import");
            }
        }

        // Hàm kiểm tra dữ liệu
        private void ValidateData(List<CHI_TIEU> chiTieuList, List<string> warnings)
        {
            // 1. Kiểm tra mã trùng trong file
            var duplicates = chiTieuList.GroupBy(x => x.MA_CHI_TIEU)
                                        .Where(g => g.Count() > 1)
                                        .Select(g => g.Key)
                                        .ToList();

            if (duplicates.Any())
            {
                warnings.Add($"Có {duplicates.Count} mã chỉ tiêu bị trùng trong file: {string.Join(", ", duplicates)}");
            }

            // 2. Kiểm tra mã đã tồn tại trong CSDL
            var existingMa = _db.CHI_TIEU.Select(x => x.MA_CHI_TIEU).ToList();
            var existingInDb = chiTieuList.Where(x => existingMa.Contains(x.MA_CHI_TIEU))
                                          .Select(x => x.MA_CHI_TIEU)
                                          .ToList();

            if (existingInDb.Any())
            {
                warnings.Add($"Có {existingInDb.Count} mã chỉ tiêu đã tồn tại trong CSDL và sẽ được cập nhật: {string.Join(", ", existingInDb.Take(5))}" +
                           (existingInDb.Count > 5 ? "..." : ""));
            }

            // 3. Kiểm tra dữ liệu bắt buộc
            for (int i = 0; i < chiTieuList.Count; i++)
            {
                var item = chiTieuList[i];

                if (string.IsNullOrWhiteSpace(item.MA_CHI_TIEU))
                {
                    warnings.Add($"Dòng {i + 2}: Thiếu mã chỉ tiêu");
                }

                if (string.IsNullOrWhiteSpace(item.TEN_CHI_TIEU))
                {
                    warnings.Add($"Dòng {i + 2} (Mã: {item.MA_CHI_TIEU}): Thiếu tên chỉ tiêu");
                }

                // Kiểm tra độ dài
                if (item.MA_CHI_TIEU?.Length > 15)
                {
                    warnings.Add($"Dòng {i + 2}: Mã chỉ tiêu vượt quá 15 ký tự");
                }

                if (item.TEN_CHI_TIEU?.Length > 200)
                {
                    warnings.Add($"Dòng {i + 2}: Tên chỉ tiêu vượt quá 200 ký tự");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}