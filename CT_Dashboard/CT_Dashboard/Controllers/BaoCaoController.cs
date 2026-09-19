using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using OfficeOpenXml;
using CT_Dashboard.Filters;
using CT_Dashboard.Models;
using System.Data.Entity;
using PagedList;


namespace CT_Dashboard.Controllers
{
    [AuthorizeRole("Admin", "User","Leader")]
    //[AuthorizeRole("Admin")]
    public class BaoCaoController : Controller
    {
        private readonly CT_DASHBOARDEntities db = new CT_DASHBOARDEntities();
        private readonly ExcelValidator _validator = new ExcelValidator();
                
        public ActionResult Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var baocaos = db.BAO_CAO.AsQueryable();
            var user = Session["User"] as CT_Dashboard.Models.NGUOI_DUNG;

            if (!string.IsNullOrEmpty(searchString))
            {
                baocaos = baocaos.Where(c => c.TEN_BC.Contains(searchString) || c.NAM.Equals(searchString) || c.THANG.Equals(searchString) || c.NGAY.Equals(searchString)).Where(c=>c.NGUOI_TAO == user.ID);
                ViewBag.SearchString = searchString;
            }
            
            if (user.VAI_TRO == "Admin")
            {
                baocaos = baocaos.OrderByDescending(c => c.THANG).ThenBy(c => c.NAM);
            }
            else
            {
                baocaos = baocaos.Where(m => m.NGUOI_TAO == user.ID).OrderByDescending(c => c.THANG);
            }
            return View(baocaos.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult IndexChiTiet(int id, string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var baocaos = db.BAO_CAO_CHI_TIET.AsQueryable();
            var user = Session["User"] as CT_Dashboard.Models.NGUOI_DUNG;
            var donvi = db.NGUOI_DUNG.Find(user.ID).DON_VI;

            if (!string.IsNullOrEmpty(searchString))
            {
                baocaos = baocaos.Where(c => c.DIA_BAN1.TEN_DIA_BAN.Contains(searchString) || c.CHI_TIEU1.TEN_CHI_TIEU.Contains(searchString)).Where(m => m.BAO_CAO == id && m.NGUOI_TAO == user.ID);
                ViewBag.SearchString = searchString;
            }
            if (user.VAI_TRO == "Admin")
            {
                baocaos = baocaos.Where(m => m.BAO_CAO == id).OrderByDescending(c => c.CHI_TIEU1.VI_TRI).ThenBy(c => c.DIA_BAN1.VI_TRI);
            }
            else
            {
                baocaos = baocaos.Where(m => m.BAO_CAO == id && m.BAO_CAO1.DON_VI == donvi).OrderByDescending(c => c.DIA_BAN1.VI_TRI);
                //baocaos = baocaos.Where(m => m.DON_VI == donvi && m.TRANG_THAI_DUYET == "TrinhLanhDao").OrderByDescending(c => c.THANG).ThenBy(c => c.NAM);
            }
            ViewBag.ID_BAO_CAO = id;
            ViewBag.TEN_BAO_CAO = db.BAO_CAO.Find(id).TEN_BC;
            return View(baocaos.ToPagedList(pageNumber, pageSize));
        }

        //[Authorize(Roles = "User")]
        public ActionResult Create()
        {
            ViewBag.DON_VI = new SelectList(db.DON_VI, "ID", "TEN_DON_VI");
            //ViewBag.CHI_TIEU = new SelectList(db.CHI_TIEU, "ID", "TEN_CHI_TIEU");
            return View();
        }

        //// POST: BaoCao/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "User")]
        public ActionResult Create(BAO_CAO baoCao)
        {
            int userId = (int)(Session["User"] as NGUOI_DUNG)?.ID;

            if (ModelState.IsValid)
            {
                if (baoCao.NGAY == null) baoCao.NGAY = DateTime.Now;
                baoCao.NGAY_TAO = DateTime.Now;
                //baoCao.NGUOI_TAO = db.NGUOI_DUNG.SingleOrDefault(u => u.USERNAME == User.Identity.Name)?.ID;
                baoCao.NGUOI_TAO = db.NGUOI_DUNG.Find(userId).ID;
                //baoCao.DON_VI = db.NGUOI_DUNG.SingleOrDefault(u => u.USERNAME == User.Identity.Name)?.DON_VI;
                baoCao.DON_VI = db.NGUOI_DUNG.Find(userId).DON_VI;
                baoCao.TRANG_THAI = "Nhap";
                baoCao.TRANG_THAI_DUYET = "Nhap";
                //baoCao.IS_LATEST = false;
                baoCao.TEN_BC = "Báo cáo tháng " + baoCao.THANG + ", Năm " + baoCao.NAM + ", Phòng " + baoCao.DON_VI;
                
                db.BAO_CAO.Add(baoCao);
                db.SaveChanges();
                return RedirectToAction("Index");
                //return RedirectToAction("Import", new { id = baoCao.ID });
            }
            //ViewBag.DON_VI = new SelectList(db.DON_VI, "ID", "TEN_DON_VI", baoCao.DON_VI);
            //ViewBag.CHI_TIEU = new SelectList(db.CHI_TIEU, "ID", "TEN_CHI_TIEU", baoCao.CHI_TIEU);
            return View(baoCao);
        }      
        
        public ActionResult Edit(int id)
        {
            var baoCao = db.BAO_CAO.Find(id);
            if (baoCao == null)
                return HttpNotFound();

            // Kiểm tra trạng thái duyệt trước khi cho phép sửa
            if (baoCao.TRANG_THAI_DUYET == "DaDuyet")
            {
                TempData["Error"] = "Không thể chỉnh sửa báo cáo đã được duyệt!";
                return RedirectToAction("Index");
            }

            // Utiliser MA_DON_VI comme ValueField (cohérent dans GET et POST)
            ViewBag.DON_VI = new SelectList(db.DON_VI, "MA_DON_VI", "TEN_DON_VI", baoCao.DON_VI);
            return View(baoCao);
        }

        // POST: BaoCao/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public ActionResult Edit(BAO_CAO baoCao)
        {
            // Validation de l'utilisateur connecté
            var currentUser = Session["User"] as NGUOI_DUNG;
            if (currentUser?.ID == null)
            {
                ViewBag.Error = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }
            int userId = currentUser.ID;

            if (ModelState.IsValid)
            {
                try
                {
                    // Récupérer l'enregistrement existant depuis la base de données
                    var existingBaoCao = db.BAO_CAO.Find(baoCao.ID);
                    if (existingBaoCao == null)
                    {
                        ViewBag.Error = "Không tìm thấy báo cáo cần chỉnh sửa.";
                        return RedirectToAction("Index");
                    }

                    // Kiểm tra lại trạng thái duyệt trước khi cập nhật
                    if (existingBaoCao.TRANG_THAI_DUYET == "DaDuyet")
                    {
                        TempData["Error"] = "Không thể chỉnh sửa báo cáo đã được duyệt!";
                        return RedirectToAction("Index");
                    }

                    // Récupérer les informations utilisateur de manière sécurisée
                    var nguoiDung = db.NGUOI_DUNG.Find(userId);
                    if (nguoiDung == null)
                    {
                        ViewBag.Error = "Không tìm thấy thông tin người dùng.";
                        ViewBag.DON_VI = new SelectList(db.DON_VI, "MA_DON_VI", "TEN_DON_VI", baoCao.DON_VI);
                        return View(baoCao);
                    }

                    // Mettre à jour uniquement les champs modifiables
                    existingBaoCao.TEN_BC = baoCao.TEN_BC;
                    existingBaoCao.THANG = baoCao.THANG;
                    existingBaoCao.NAM = baoCao.NAM;
                    existingBaoCao.NGAY = baoCao.NGAY;
                    existingBaoCao.DIEN_GIAI = baoCao.DIEN_GIAI;
                    //existingBaoCao.TRANG_THAI = baoCao.TRANG_THAI;
                    //existingBaoCao.TRANG_THAI_DUYET = baoCao.TRANG_THAI_DUYET;
                    // Mettre à jour les champs de suivi
                    //existingBaoCao.NGAY_DUYET = DateTime.Now;
                    //existingBaoCao.NGUOI_DUYET = userId; // Utiliser directement l'ID
                    //existingBaoCao.DON_VI = nguoiDung.DON_VI;

                    // Marquer comme modifié et sauvegarder
                    db.Entry(existingBaoCao).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật báo cáo thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = $"Lỗi khi cập nhật báo cáo: {ex.Message}";
                    // Log l'erreur si nécessaire
                    // Logger.LogError(ex, "Error updating BaoCao with ID: {Id}", baoCao.ID);
                }
            }
            else
            {
                // Afficher les erreurs de validation
                ViewBag.ValidationErrors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();
            }
            // Recharger la SelectList en cas d'erreur (cohérent avec GET)
            ViewBag.DON_VI = new SelectList(db.DON_VI, "MA_DON_VI", "TEN_DON_VI", baoCao.DON_VI);
            return View(baoCao);
        }

        // GET: BaoCao/Delete/5
        public ActionResult Delete(int id)
        {
            var baoCao = db.BAO_CAO.Find(id);
            if (baoCao == null)
                return HttpNotFound();

            // Kiểm tra trạng thái duyệt trước khi cho phép xóa
            if (baoCao.TRANG_THAI_DUYET == "DaDuyet")
            {
                TempData["Error"] = "Không thể xóa báo cáo đã được duyệt!";
                return RedirectToAction("Index");
            }

            return View(baoCao);
        }

        // POST: BaoCao/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var baoCao = db.BAO_CAO.Find(id);
                    if (baoCao == null)
                    {
                        TempData["Error"] = "Không tìm thấy báo cáo cần xóa.";
                        return RedirectToAction("Index");
                    }

                    // Kiểm tra lại trạng thái duyệt trước khi xóa
                    if (baoCao.TRANG_THAI_DUYET == "DaDuyet")
                    {
                        TempData["Error"] = "Không thể xóa báo cáo đã được duyệt!";
                        return RedirectToAction("Index");
                    }

                    // Xóa tất cả báo cáo chi tiết trước
                    var baoCaoChiTiets = db.BAO_CAO_CHI_TIET.Where(x => x.BAO_CAO == id).ToList();
                    if (baoCaoChiTiets.Any())
                    {
                        db.BAO_CAO_CHI_TIET.RemoveRange(baoCaoChiTiets);
                        db.SaveChanges();
                    }

                    // Sau đó xóa báo cáo chính
                    db.BAO_CAO.Remove(baoCao);
                    db.SaveChanges();

                    // Commit transaction nếu tất cả thao tác thành công
                    transaction.Commit();

                    TempData["Success"] = $"Xóa báo cáo và {baoCaoChiTiets.Count} chi tiết báo cáo thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi
                    transaction.Rollback();
                    TempData["Error"] = $"Lỗi khi xóa báo cáo và chi tiết: {ex.Message}";
                    return RedirectToAction("Index");
                }
            }
        }

        public ActionResult Import(int id)
        {
            ViewBag.ID_BAO_CAO = id;
            return View();
        }

        [HttpPost]
        public ActionResult Import(int id, HttpPostedFileBase file, string action)
        {
            if (file == null || file.ContentLength == 0)
            {
                ViewBag.Error = "Vui lòng chọn file Excel.";
                ViewBag.ID_BAO_CAO = id;
                return View();
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Vui lòng chọn file Excel (.xlsx).";
                ViewBag.ID_BAO_CAO = id;
                return View();
            }

            try
            {
                // Validation de l'utilisateur
                var currentUser = Session["User"] as NGUOI_DUNG;
                if (currentUser?.ID == null)
                {
                    ViewBag.Error = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                    ViewBag.ID_BAO_CAO = id;
                    return View();
                }

                int userId = currentUser.ID;
                string donvi_user = db.NGUOI_DUNG.Find(userId)?.DON_VI;

                if (string.IsNullOrEmpty(donvi_user))
                {
                    ViewBag.Error = "Không tìm thấy thông tin đơn vị của người dùng.";
                    ViewBag.ID_BAO_CAO = id;
                    return View();
                }

                // Définir le chemin du dossier de téléchargement
                string uploadFolder = Server.MapPath("~/Files/Uploads/");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Créer un nom de fichier unique
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{timestamp}_{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                string filePath = Path.Combine(uploadFolder, fileName);

                // Sauvegarder le fichier sur le serveur
                file.SaveAs(filePath);

                // Variables pour suivre le traitement
                int totalRows = 0;
                int successfulRows = 0;
                int skippedRows = 0;
                List<string> errorMessages = new List<string>();

                // Lire et traiter le fichier Excel
                using (var stream = new MemoryStream())
                {
                    file.InputStream.Position = 0; // Reset stream position
                    file.InputStream.CopyTo(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                        if (worksheet == null)
                        {
                            ViewBag.Error = "Không tìm thấy worksheet trong file Excel.";
                            ViewBag.ID_BAO_CAO = id;
                            return View();
                        }

                        if (worksheet.Dimension == null)
                        {
                            ViewBag.Error = "File Excel trống hoặc không có dữ liệu.";
                            ViewBag.ID_BAO_CAO = id;
                            return View();
                        }

                        var rowCount = worksheet.Dimension.Rows;
                        totalRows = rowCount - 1; // Trừ header row

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                string machitieu = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

                                // Kiểm tra mã chỉ tiêu có tồn tại không
                                if (string.IsNullOrEmpty(machitieu))
                                {
                                    errorMessages.Add($"Dòng {row}: Mã chỉ tiêu trống.");
                                    skippedRows++;
                                    continue;
                                }

                                var chiTieu = db.CHI_TIEU.FirstOrDefault(u => u.MA_CHI_TIEU == machitieu);
                                if (chiTieu == null)
                                {
                                    errorMessages.Add($"Dòng {row}: Không tìm thấy chỉ tiêu '{machitieu}'.");
                                    skippedRows++;
                                    continue;
                                }

                                string donvi = chiTieu.DON_VI;

                                if (donvi != donvi_user)
                                {
                                    errorMessages.Add($"Dòng {row}: Người dùng không có quyền cập nhật chỉ tiêu '{machitieu}'.");
                                    skippedRows++;
                                    continue;
                                }

                                // Kiểm tra xem bản ghi đã tồn tại chưa
                                string diaBan = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                var existingRecord = db.BAO_CAO_CHI_TIET
                                    .FirstOrDefault(x => x.BAO_CAO == id &&
                                                       x.CHI_TIEU == machitieu &&
                                                       x.DIA_BAN == diaBan);

                                if (donvi_user == "NVDTPC")
                                {
                                    if (existingRecord != null)
                                    {
                                        // Cập nhật bản ghi hiện tại
                                        existingRecord.THUC_HIEN_TRONG_KY = double.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out double thucHientk) ? (double?)thucHientk : null;
                                        existingRecord.THUC_HIEN = double.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out double thucHien) ? (double?)thucHien : null;
                                        existingRecord.KE_HOACH = double.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out double keHoach) ? (double?)keHoach : null;
                                        existingRecord.CUNG_KY = double.TryParse(worksheet.Cells[row, 7].Value?.ToString(), out double cungky) ? (double?)cungky : null;
                                        //existingRecord.DVT = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
                                        existingRecord.NGUOI_SUA = userId;
                                        existingRecord.NGAY_SUA = DateTime.Now;
                                        existingRecord.DIEN_GIAI = worksheet.Cells[row, 12].Value?.ToString()?.Trim();

                                        db.Entry(existingRecord).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        // Tạo bản ghi mới
                                        var baoCaoChiTiet = new BAO_CAO_CHI_TIET
                                        {
                                            BAO_CAO = id,
                                            DIA_BAN = diaBan,
                                            CHI_TIEU = machitieu,
                                            THUC_HIEN_TRONG_KY = double.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out double thucHientk) ? (double?)thucHientk : null,
                                            THUC_HIEN = double.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out double thucHien) ? (double?)thucHien : null,
                                            KE_HOACH = double.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out double keHoach) ? (double?)keHoach : null,
                                            CUNG_KY = double.TryParse(worksheet.Cells[row, 7].Value?.ToString(), out double cungky) ? (double?)cungky : null,
                                            //DVT = worksheet.Cells[row, 8].Value?.ToString()?.Trim(),
                                            NGUOI_TAO = userId,
                                            NGAY_TAO = DateTime.Now,
                                            NGUOI_SUA = userId,
                                            NGAY_SUA = DateTime.Now,
                                            DIEN_GIAI = worksheet.Cells[row, 12].Value?.ToString()?.Trim(),
                                        };
                                        db.BAO_CAO_CHI_TIET.Add(baoCaoChiTiet);
                                    }
                                }
                                else
                                {
                                    if (existingRecord != null)
                                    {
                                        // Cập nhật bản ghi hiện tại
                                        //existingRecord.THUC_HIEN_TRONG_KY = double.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out double thucHientk) ? (double?)thucHientk : null;
                                        existingRecord.THUC_HIEN = double.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out double thucHien) ? (double?)thucHien : null;
                                        //existingRecord.KE_HOACH = double.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out double keHoach) ? (double?)keHoach : null;
                                        //existingRecord.CUNG_KY = double.TryParse(worksheet.Cells[row, 7].Value?.ToString(), out double cungky) ? (double?)cungky : null;
                                        //existingRecord.DVT = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
                                        existingRecord.NGUOI_SUA = userId;
                                        existingRecord.NGAY_SUA = DateTime.Now;
                                        existingRecord.DIEN_GIAI = worksheet.Cells[row, 6].Value?.ToString()?.Trim();

                                        db.Entry(existingRecord).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        // Tạo bản ghi mới
                                        var baoCaoChiTiet = new BAO_CAO_CHI_TIET
                                        {
                                            BAO_CAO = id,
                                            DIA_BAN = diaBan,
                                            CHI_TIEU = machitieu,
                                            //THUC_HIEN_TRONG_KY = double.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out double thucHientk) ? (double?)thucHientk : null,
                                            THUC_HIEN = double.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out double thucHien) ? (double?)thucHien : null,
                                            //KE_HOACH = double.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out double keHoach) ? (double?)keHoach : null,
                                            //CUNG_KY = double.TryParse(worksheet.Cells[row, 7].Value?.ToString(), out double cungky) ? (double?)cungky : null,
                                            //DVT = worksheet.Cells[row, 8].Value?.ToString()?.Trim(),
                                            NGUOI_TAO = userId,
                                            NGAY_TAO = DateTime.Now,
                                            NGUOI_SUA = userId,
                                            NGAY_SUA = DateTime.Now,
                                            DIEN_GIAI = worksheet.Cells[row, 6].Value?.ToString()?.Trim(),
                                        };
                                        db.BAO_CAO_CHI_TIET.Add(baoCaoChiTiet);
                                    }
                                }

                                successfulRows++;
                            }
                            catch (Exception rowEx)
                            {
                                errorMessages.Add($"Dòng {row}: {rowEx.Message}");
                                skippedRows++;
                            }
                        }

                        // Lưu thay đổi cho chi tiết báo cáo
                        db.SaveChanges();

                        // Cập nhật trạng thái báo cáo
                        var baoCao = db.BAO_CAO.Find(id);
                        if (baoCao != null)
                        {
                            baoCao.TRANG_THAI = "HasDetail";
                            //baoCao.NGAY_DUYET = DateTime.Now;
                            //baoCao.NGUOI_DUYET = userId;
                            db.Entry(baoCao).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        // Tạo thông báo kết quả
                        if (successfulRows > 0)
                        {
                            ViewBag.Success = $"Nhập dữ liệu thành công! Đã xử lý: {successfulRows}/{totalRows} dòng.";

                            if (skippedRows > 0)
                            {
                                ViewBag.Warning = $"Có {skippedRows} dòng bị bỏ qua.";
                                ViewBag.ErrorDetails = errorMessages;
                            }
                        }
                        else
                        {
                            ViewBag.Error = "Xử lý không thành công, có thể là do bạn nhập sai mã chỉ tiêu hoặc nhập chỉ tiêu không thuộc đơn vị quản lý...";
                            ViewBag.ErrorDetails = errorMessages;
                        }

                        // Xóa file tạm sau khi xử lý (tùy chọn)
                        try
                        {
                            if (System.IO.File.Exists(filePath))
                                System.IO.File.Delete(filePath);
                        }
                        catch { /* Ignore cleanup errors */ }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi khi xử lý file: {ex.Message}";

                // Log chi tiết lỗi nếu cần
                if (ex.InnerException != null)
                {
                    ViewBag.Error += $" Chi tiết: {ex.InnerException.Message}";
                }
            }

            ViewBag.ID_BAO_CAO = id;
            return View();
        }

        // GET: Hiển thị form xác nhận chuyển lãnh đạo
        //[AuthorizeRole("User")]
        public ActionResult ChuyenLanhDao(int id)
        {
            var baoCao = db.BAO_CAO.Find(id);
            if (baoCao == null)
                return HttpNotFound();

            // Kiểm tra trạng thái duyệt - sửa logic kiểm tra
            if (baoCao.TRANG_THAI_DUYET == "TrinhLanhDao")
            {
                TempData["Error"] = "Báo cáo đã được chuyển lãnh đạo rồi!";
                return RedirectToAction("Index");
            }

            if (baoCao.TRANG_THAI_DUYET == "DaDuyet")
            {
                TempData["Error"] = "Không thể chuyển báo cáo đã được duyệt!";
                return RedirectToAction("Index");
            }

            // Kiểm tra trạng thái chi tiết báo cáo
            if (baoCao.TRANG_THAI != "HasDetail")
            {
                TempData["Error"] = "Không thể chuyển báo cáo do chưa nhập chi tiết báo cáo!";
                return RedirectToAction("Index");
            }

            return View(baoCao);
        }

        // POST: Xử lý chuyển lãnh đạo
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[AuthorizeRole("User")]
        public ActionResult ChuyenLanhDao(int id, FormCollection form)
        {
            var currentUser = Session["User"] as NGUOI_DUNG;
            if (currentUser?.ID == null)
            {
                TempData["Error"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var existingBaoCao = db.BAO_CAO.Find(id);
                if (existingBaoCao == null)
                {
                    TempData["Error"] = "Không tìm thấy báo cáo cần chuyển.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra lại trạng thái duyệt trước khi cập nhật
                if (existingBaoCao.TRANG_THAI_DUYET == "DaDuyet")
                {
                    TempData["Error"] = "Không thể chuyển báo cáo đã được duyệt!";
                    return RedirectToAction("Index");
                }

                // Kiểm tra lại trạng thái đã nhập báo cáo chi tiết
                if (existingBaoCao.TRANG_THAI != "HasDetail")
                {
                    TempData["Error"] = "Không thể chuyển báo cáo chưa có nội dung, cần phải nhập chi tiết báo cáo!";
                    return RedirectToAction("Index");
                }

                existingBaoCao.TRANG_THAI_DUYET = "TrinhLanhDao";

                db.Entry(existingBaoCao).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Đã chuyển báo cáo cho lãnh đạo đơn vị thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi chuyển báo cáo cho lãnh đạo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }      

        [AuthorizeRole("Leader")]
        public ActionResult DuyetBaoCao(string searchString, int? page)
        {
            var user = Session["User"] as CT_Dashboard.Models.NGUOI_DUNG;
            if (user.VAI_TRO == "Leader")
            {
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                var baocaos = db.BAO_CAO.AsQueryable();
                int userId = (int)(Session["User"] as NGUOI_DUNG)?.ID;
                var donvi = db.NGUOI_DUNG.Find(userId).DON_VI;
                baocaos = baocaos.Where(m => m.DON_VI == donvi && m.TRANG_THAI_DUYET == "TrinhLanhDao").OrderByDescending(c => c.THANG).ThenBy(c => c.NAM);
                return View(baocaos.ToPagedList(pageNumber, pageSize));
            }
            return RedirectToAction("Index");
        }

        // GET: Hiển thị form duyệt báo cáo
        [AuthorizeRole("Leader")]
        public ActionResult DuyetBaoCaoForm(int id)
        {
            try
            {
                // Debug: Kiểm tra ID
                if (id <= 0)
                {
                    TempData["Error"] = "ID báo cáo không hợp lệ.";
                    return RedirectToAction("DuyetBaoCao");
                }

                // Lấy báo cáo với Include để load navigation properties
                var baoCao = db.BAO_CAO
                    .Include("BAO_CAO_CHI_TIET")
                    .Where(x => x.ID == id)
                    .FirstOrDefault();

                // Debug: Kiểm tra null
                if (baoCao == null)
                {
                    TempData["Error"] = $"Không tìm thấy báo cáo với ID: {id}";
                    return RedirectToAction("DuyetBaoCao");
                }

                var currentUser = Session["User"] as NGUOI_DUNG;
                if (currentUser?.ID == null)
                {
                    TempData["Error"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Login", "Account");
                }

                var userInfo = db.NGUOI_DUNG.Find(currentUser.ID);
                if (userInfo == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction("Login", "Account");
                }

                var donvi = userInfo.DON_VI;

                // Kiểm tra quyền duyệt (cùng đơn vị)
                if (baoCao.DON_VI != donvi)
                {
                    TempData["Error"] = "Bạn không có quyền duyệt báo cáo này!";
                    return RedirectToAction("DuyetBaoCao");
                }

                // Kiểm tra trạng thái
                if (baoCao.TRANG_THAI_DUYET != "TrinhLanhDao")
                {
                    TempData["Error"] = "Chỉ duyệt báo cáo trong trạng thái trình lãnh đạo!";
                    return RedirectToAction("DuyetBaoCao");
                }

                // Debug: Kiểm tra type trước khi return
                System.Diagnostics.Debug.WriteLine($"Model type: {baoCao.GetType().FullName}");

                return View(baoCao);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("DuyetBaoCao");
            }
        }

        // POST: Duyệt báo cáo
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Leader")]
        public ActionResult DuyetBaoCaoForm(int id, string action, string lyDoTuChoi = "")
        {
            var currentUser = Session["User"] as NGUOI_DUNG;
            if (currentUser?.ID == null)
            {
                TempData["Error"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var existingBaoCao = db.BAO_CAO.Find(id);
                if (existingBaoCao == null)
                {
                    TempData["Error"] = "Không tìm thấy báo cáo cần xử lý.";
                    return RedirectToAction("DuyetBaoCao");
                }

                // Kiểm tra quyền
                var donvi = db.NGUOI_DUNG.Find(currentUser.ID).DON_VI;
                if (existingBaoCao.DON_VI != donvi)
                {
                    TempData["Error"] = "Bạn không có quyền xử lý báo cáo này!";
                    return RedirectToAction("DuyetBaoCao");
                }

                // Kiểm tra trạng thái
                if (existingBaoCao.TRANG_THAI_DUYET != "TrinhLanhDao")
                {
                    TempData["Error"] = "Chỉ xử lý báo cáo trong trạng thái trình lãnh đạo!";
                    return RedirectToAction("DuyetBaoCao");
                }

                // Kiểm tra chi tiết báo cáo
                if (existingBaoCao.TRANG_THAI == "NoDetail")
                {
                    TempData["Error"] = "Không thể xử lý báo cáo chưa có nội dung!";
                    return RedirectToAction("DuyetBaoCao");
                }

                if (action == "duyet")
                {
                    // Duyệt báo cáo
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            // 1. Cập nhật tất cả báo cáo cùng năm, tháng, đơn vị thành IS_LATEST = false
                            var oldReports = db.BAO_CAO.Where(x =>
                                x.NAM == existingBaoCao.NAM &&
                                x.THANG == existingBaoCao.THANG &&
                                x.DON_VI == existingBaoCao.DON_VI &&
                                x.ID != existingBaoCao.ID &&
                                x.IS_LATEST == true).ToList();

                            foreach (var oldReport in oldReports)
                            {
                                oldReport.IS_LATEST = false;
                                db.Entry(oldReport).State = EntityState.Modified;
                            }

                            // 2. Cập nhật báo cáo hiện tại
                            existingBaoCao.TRANG_THAI_DUYET = "DaDuyet";
                            existingBaoCao.IS_LATEST = true;
                            existingBaoCao.NGAY_DUYET = DateTime.Now;
                            existingBaoCao.NGUOI_DUYET = currentUser.ID;

                            db.Entry(existingBaoCao).State = EntityState.Modified;
                            db.SaveChanges();

                            // Commit transaction
                            transaction.Commit();

                            TempData["Success"] = $"Đã duyệt báo cáo thành công! {oldReports.Count} báo cáo cũ đã được cập nhật trạng thái.";
                        }
                        catch (Exception transactionEx)
                        {
                            // Rollback nếu có lỗi
                            transaction.Rollback();
                            throw new Exception($"Lỗi khi cập nhật IS_LATEST: {transactionEx.Message}");
                        }
                    }
                }
                else if (action == "tuchoi")
                {
                    // Từ chối báo cáo - KHÔNG cập nhật IS_LATEST
                    if (string.IsNullOrWhiteSpace(lyDoTuChoi))
                    {
                        TempData["Error"] = "Vui lòng nhập lý do từ chối!";
                        return View(existingBaoCao);
                    }

                    existingBaoCao.TRANG_THAI_DUYET = "TuChoi";
                    existingBaoCao.NGAY_DUYET = DateTime.Now;
                    existingBaoCao.NGUOI_DUYET = currentUser.ID;
                    existingBaoCao.LY_DO_TU_CHOI = lyDoTuChoi;
                    // Không thay đổi IS_LATEST khi từ chối

                    db.Entry(existingBaoCao).State = EntityState.Modified;
                    db.SaveChanges();

                    TempData["Success"] = "Đã từ chối báo cáo!";
                }

                return RedirectToAction("DuyetBaoCao");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xử lý báo cáo: {ex.Message}";
                return RedirectToAction("DuyetBaoCao");
            }
        }

        // POST: Duyệt nhanh từ danh sách (giữ nguyên cho tương thích)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Leader")]
        public ActionResult DuyetNhanh(int id)
        {
            return DuyetBaoCaoForm(id, "duyet");
        }

        [AuthorizeRole("Leader")]
        public ActionResult BaoCaoDaDuyet(string searchString, int? page)
        {
            var user = Session["User"] as CT_Dashboard.Models.NGUOI_DUNG;
            if (user.VAI_TRO == "Leader")
            {
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                var baocaos = db.BAO_CAO.AsQueryable();

                int userId = (int)(Session["User"] as NGUOI_DUNG)?.ID;
                var donvi = db.NGUOI_DUNG.Find(userId).DON_VI;

                if (!string.IsNullOrEmpty(searchString))
                {
                    baocaos = baocaos.Where(c => c.TEN_BC.Contains(searchString) || c.NAM.Equals(searchString) || c.THANG.Equals(searchString) || c.NGAY.Equals(searchString)).Where(m => m.DON_VI == donvi && (m.TRANG_THAI_DUYET == "DaDuyet" || m.TRANG_THAI_DUYET == "TuChoi"));
                    ViewBag.SearchString = searchString;
                }

                baocaos = baocaos.Where(m => m.DON_VI == donvi && (m.TRANG_THAI_DUYET == "DaDuyet" || m.TRANG_THAI_DUYET == "TuChoi")).OrderByDescending(c => c.THANG).ThenBy(c => c.NAM);

                return View(baocaos.ToPagedList(pageNumber, pageSize));
            }
            return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }        

        public ActionResult DownloadSample()
        {
            string path = Server.MapPath("~/Files/SampleBaoCao.xlsx");
            return File(path, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SampleBaoCao.xlsx");
        }
    }
}