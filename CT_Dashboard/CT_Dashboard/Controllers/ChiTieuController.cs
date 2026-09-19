using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using CT_Dashboard.Models;
using PagedList;
using CT_Dashboard.Filters;
using OfficeOpenXml;
using System.IO;
using System.Web;
using System.Collections.Generic;

namespace CT_Dashboard.Controllers
{
    //[AuthorizeRole("Admin", "User")]
    [AuthorizeRole("Admin")]
    public class ChiTieuController : Controller
    {
        private readonly CT_DASHBOARDEntities db = new CT_DASHBOARDEntities();

        // GET: ChiTieu
        public ActionResult Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var chiTieus = db.CHI_TIEU.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                chiTieus = chiTieus.Where(c => c.MA_CHI_TIEU.Contains(searchString) || c.TEN_CHI_TIEU.Contains(searchString));
                ViewBag.SearchString = searchString;
            }
            chiTieus = chiTieus.OrderBy(c => c.PARENTID).ThenBy(c => c.VI_TRI);
            return View(chiTieus.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                ViewBag.Error = "Vui lòng chọn file Excel.";
                return View();
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Vui lòng chọn file Excel (.xlsx).";
                return View();
            }

            try
            {
                int totalRows = 0;
                int addedRows = 0;
                int updatedRows = 0;
                int skippedRows = 0;
                List<string> errorMessages = new List<string>();

                using (var stream = new MemoryStream())
                {
                    file.InputStream.Position = 0; // Reset stream position
                    file.InputStream.CopyTo(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets["ChiTieu"];

                        if (worksheet == null)
                        {
                            ViewBag.Error = "Không tìm thấy sheet 'ChiTieu' trong file Excel.";
                            return View();
                        }

                        if (worksheet.Dimension == null)
                        {
                            ViewBag.Error = "File Excel trống hoặc không có dữ liệu.";
                            return View();
                        }

                        var rowCount = worksheet.Dimension.Rows;
                        totalRows = rowCount - 1; // Trừ header row

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                // Lấy mã chỉ tiêu (khóa chính)
                                string maChiTieu = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

                                if (string.IsNullOrEmpty(maChiTieu))
                                {
                                    errorMessages.Add($"Dòng {row}: Mã chỉ tiêu trống, bỏ qua.");
                                    skippedRows++;
                                    continue;
                                }

                                // Kiểm tra xem bản ghi đã tồn tại chưa
                                var existingChiTieu = db.CHI_TIEU.FirstOrDefault(x => x.MA_CHI_TIEU == maChiTieu);

                                // Đọc dữ liệu từ Excel
                                string tenChiTieu = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                string parentId = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                                byte viTri = byte.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out byte vt) ? vt : (byte)1;
                                string donVi = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                                string dvt = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                                string maMau = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
                                string trangThai = worksheet.Cells[row, 9].Value?.ToString()?.Trim();

                                // Xử lý boolean cho QUAN_TRONG
                                bool quanTrong = false;
                                var quanTrongValue = worksheet.Cells[row, 10].Value?.ToString()?.Trim().ToLower();
                                if (!string.IsNullOrEmpty(quanTrongValue))
                                {
                                    quanTrong = quanTrongValue == "true" ||
                                               quanTrongValue == "1" ||
                                               quanTrongValue == "yes" ||
                                               quanTrongValue == "có";
                                }

                                string tenDayDu = worksheet.Cells[row, 11].Value?.ToString()?.Trim();

                                if (existingChiTieu != null)
                                {
                                    // CẬP NHẬT bản ghi hiện có
                                    existingChiTieu.TEN_CHI_TIEU = tenChiTieu ?? existingChiTieu.TEN_CHI_TIEU;
                                    existingChiTieu.PARENTID = parentId ?? existingChiTieu.PARENTID;
                                    existingChiTieu.VI_TRI = viTri;
                                    existingChiTieu.DON_VI = donVi ?? existingChiTieu.DON_VI;
                                    existingChiTieu.DVT = dvt ?? existingChiTieu.DVT;
                                    existingChiTieu.MA_MAU = maMau ?? existingChiTieu.MA_MAU;
                                    existingChiTieu.TRANG_THAI = trangThai ?? existingChiTieu.TRANG_THAI;
                                    existingChiTieu.QUAN_TRONG = quanTrong;
                                    existingChiTieu.TEN_DAY_DU = tenDayDu ?? existingChiTieu.TEN_DAY_DU;

                                    db.Entry(existingChiTieu).State = EntityState.Modified;
                                    updatedRows++;
                                }
                                else
                                {
                                    // THÊM MỚI bản ghi
                                    var chiTieu = new CHI_TIEU
                                    {
                                        MA_CHI_TIEU = maChiTieu,
                                        TEN_CHI_TIEU = tenChiTieu,
                                        PARENTID = parentId,
                                        VI_TRI = viTri,
                                        DON_VI = donVi,
                                        DVT = dvt,
                                        MA_MAU = maMau,
                                        TRANG_THAI = trangThai,
                                        QUAN_TRONG = quanTrong,
                                        TEN_DAY_DU = tenDayDu
                                    };

                                    db.CHI_TIEU.Add(chiTieu);
                                    addedRows++;
                                }
                            }
                            catch (Exception rowEx)
                            {
                                errorMessages.Add($"Dòng {row}: {rowEx.Message}");
                                skippedRows++;
                            }
                        }

                        // Lưu tất cả thay đổi
                        db.SaveChanges();

                        // Tạo thông báo kết quả chi tiết
                        var resultMessage = new List<string>();

                        if (addedRows > 0)
                            resultMessage.Add($"<span class='text-green-600'>✓ Thêm mới: {addedRows} bản ghi</span>");

                        if (updatedRows > 0)
                            resultMessage.Add($"<span class='text-blue-600'>↻ Cập nhật: {updatedRows} bản ghi</span>");

                        if (skippedRows > 0)
                            resultMessage.Add($"<span class='text-orange-600'>⊘ Bỏ qua: {skippedRows} bản ghi</span>");

                        ViewBag.Success = $@"
                    <div class='alert alert-success'>
                        <strong>Nhập dữ liệu thành công!</strong><br/>
                        Tổng số dòng xử lý: {totalRows}<br/>
                        {string.Join("<br/>", resultMessage)}
                    </div>";

                        if (errorMessages.Any())
                        {
                            ViewBag.Warning = $@"
                        <div class='alert alert-warning'>
                            <strong>Cảnh báo:</strong><br/>
                            {string.Join("<br/>", errorMessages.Take(10))}
                            {(errorMessages.Count > 10 ? $"<br/>... và {errorMessages.Count - 10} lỗi khác" : "")}
                        </div>";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi khi xử lý file: {ex.Message}";

                if (ex.InnerException != null)
                {
                    ViewBag.Error += $"<br/>Chi tiết: {ex.InnerException.Message}";
                }
            }

            return View();
        }

        //[HttpPost]        
        //public ActionResult Import(HttpPostedFileBase file)
        //{
        //    if (file == null || file.ContentLength == 0)
        //    {
        //        ViewBag.Error = "Vui lòng chọn file Excel.";
        //        return View();
        //    }

        //    if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        //    {
        //        ViewBag.Error = "Vui lòng chọn file Excel (.xlsx).";
        //        return View();
        //    }

        //    try
        //    {
        //        using (var stream = new MemoryStream())
        //        {
        //            file.InputStream.CopyTo(stream);
        //            using (var package = new ExcelPackage(stream))
        //            {
        //                //var worksheet = package.Workbook.Worksheets[1];
        //                var worksheet = package.Workbook.Worksheets["ChiTieu"];
        //                var rowCount = worksheet.Dimension.Rows;

        //                for (int row = 2; row <= rowCount; row++)
        //                {                            
        //                    var chiTieu = new CHI_TIEU
        //                    {
        //                        MA_CHI_TIEU = worksheet.Cells[row, 2].Value?.ToString(), //donViId,
        //                        TEN_CHI_TIEU = worksheet.Cells[row, 3].Value?.ToString(),//chiTieuId,
        //                        PARENTID = worksheet.Cells[row, 4].Value?.ToString(),//chiTieuId,
        //                        VI_TRI = byte.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out byte vitri) ? vitri : (byte)1,
        //                        DON_VI = worksheet.Cells[row, 6].Value?.ToString(),//chiTieuId,
        //                        DVT = worksheet.Cells[row, 7].Value?.ToString(),//chiTieuId,
        //                        MA_MAU = worksheet.Cells[row, 8].Value?.ToString(),//chiTieuId,
        //                        TRANG_THAI = worksheet.Cells[row, 9].Value?.ToString(),//chiTieuId,
        //                        QUAN_TRONG = Convert.ToBoolean(worksheet.Cells[row, 10].Value?.ToString()),//chiTieuId,
        //                        TEN_DAY_DU = worksheet.Cells[row, 11].Value?.ToString(),//chiTieuId,
        //                    };

        //                    db.CHI_TIEU.Add(chiTieu);
        //                }
        //                db.SaveChanges();
        //                ViewBag.Success = "Nhập dữ liệu thành công!";
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Error = $"Lỗi khi xử lý file: {ex.Message}";
        //    }

        //    return View();
        //}

        // GET: ChiTieu/Create
        //[Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.CHI_TIEU = new SelectList(db.CHI_TIEU.Where(dv => dv.PARENTID == "0"), "MA_CHI_TIEU", "TEN_CHI_TIEU", null);
            ViewBag.DON_VI = new SelectList(db.DON_VI.OrderBy(m=>m.VI_TRI), "MA_DON_VI", "TEN_DON_VI", null);
            return View();
        }

        // POST: ChiTieu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public ActionResult Create(CHI_TIEU chiTieu)
        {
            if (ModelState.IsValid)
            {
                db.CHI_TIEU.Add(chiTieu);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CHI_TIEU = new SelectList(db.CHI_TIEU.Where(dv => dv.PARENTID == "0"), "MA_CHI_TIEU", "TEN_CHI_TIEU", chiTieu.PARENTID);
            ViewBag.DON_VI = new SelectList(db.DON_VI.OrderBy(m => m.VI_TRI), "MA_DON_VI", "TEN_DON_VI", chiTieu.DON_VI);
            return View(chiTieu);
        }

        // GET: ChiTieu/Edit/5
        //[Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var chiTieu = db.CHI_TIEU.Where(m => m.ID == id).FirstOrDefault();
            if (chiTieu == null) return HttpNotFound();
            ViewBag.CHI_TIEU = new SelectList(db.CHI_TIEU.Where(dv => dv.PARENTID == "0"), "MA_CHI_TIEU", "TEN_CHI_TIEU", chiTieu.PARENTID);
            ViewBag.DON_VI = new SelectList(db.DON_VI.OrderBy(m => m.VI_TRI), "MA_DON_VI", "TEN_DON_VI", chiTieu.DON_VI);
            return View(chiTieu);
        }

        // POST: ChiTieu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public ActionResult Edit(CHI_TIEU chiTieu)
        {
            if (ModelState.IsValid)
            {
                db.Entry(chiTieu).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CHI_TIEU = new SelectList(db.CHI_TIEU.Where(dv => dv.PARENTID == "0"), "MA_CHI_TIEU", "TEN_CHI_TIEU", chiTieu.PARENTID);
            ViewBag.DON_VI = new SelectList(db.DON_VI.OrderBy(m => m.VI_TRI), "MA_DON_VI", "TEN_DON_VI", chiTieu.DON_VI);
            return View(chiTieu);
        }

        // GET: ChiTieu/Delete/5
        //[Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            var chiTieu = db.CHI_TIEU.Where(m => m.ID == id).FirstOrDefault();
            if (chiTieu == null) return HttpNotFound();
            return View(chiTieu);
        }

        // POST: ChiTieu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            var chiTieu = db.CHI_TIEU.Where(m => m.ID == id).FirstOrDefault();
            db.CHI_TIEU.Remove(chiTieu);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}