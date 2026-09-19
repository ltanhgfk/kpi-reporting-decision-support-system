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
using DocumentFormat.OpenXml.Wordprocessing;

namespace CT_Dashboard.Controllers
{
    //[AuthorizeRole("Admin", "User")]
    [AuthorizeRole("Admin")]
    public class DiaBanController : Controller
    {
        private CT_DASHBOARDEntities db = new CT_DASHBOARDEntities();

        // GET: DiaBan
        public ActionResult Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var DiaBans = db.DIA_BAN.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                DiaBans = DiaBans.Where(d => d.MA_DIA_BAN.Contains(searchString) || d.TEN_DIA_BAN.Contains(searchString));
                ViewBag.SearchString = searchString;
            }
            //DiaBans = DiaBans.OrderBy(d => d.PARENTID).ThenBy(d => d.VI_TRI);
            DiaBans = DiaBans.OrderBy(d => d.VI_TRI);
            return View(DiaBans.ToPagedList(pageNumber, pageSize));
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
                using (var stream = new MemoryStream())
                {
                    file.InputStream.CopyTo(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        //var worksheet = package.Workbook.Worksheets[1];
                        var worksheet = package.Workbook.Worksheets["DiaBan"];
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {                           
                            var DiaBan = new DIA_BAN
                            {
                                MA_DIA_BAN = worksheet.Cells[row, 2].Value?.ToString(), //DiaBanId,
                                TEN_DIA_BAN = worksheet.Cells[row, 3].Value?.ToString(),//chiTieuId,
                                VI_TRI = byte.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out byte vitri) ? vitri : (byte)1,
                            };

                            db.DIA_BAN.Add(DiaBan);
                        }

                        db.SaveChanges();
                        ViewBag.Success = "Nhập dữ liệu thành công!";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi khi xử lý file: {ex.Message}";
            }

            return View();
        }


        // GET: DiaBan/Create
        //[Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            //ViewBag.PARENTID = new SelectList(db.DIA_BAN, "ID", "TEN_DIA_BAN");
            //ViewBag.PARENTID = new SelectList(db.DIA_BAN.Where(dv => dv.PARENTID == "0"), "MA_DIA_BAN", "TEN_DIA_BAN");
            return View();
        }

        // POST: DiaBan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public ActionResult Create(DIA_BAN DiaBan)
        {
            if (ModelState.IsValid)
            {
                db.DIA_BAN.Add(DiaBan);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.PARENTID = new SelectList(db.DIA_BAN, "ID", "TEN_DIA_BAN", DiaBan.PARENTID);
            //ViewBag.PARENTID = new SelectList(db.DIA_BAN.Where(dv => dv.PARENTID == "0"), "MA_DIA_BAN", "TEN_DIA_BAN", DiaBan.PARENTID);
            return View(DiaBan);
        }

        // GET: DiaBan/Edit/5
        //[Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var DiaBan = (DIA_BAN)db.DIA_BAN.Where(m => m.ID == id).FirstOrDefault();
            if (DiaBan == null) return HttpNotFound();
            //ViewBag.PARENTID = new SelectList(db.DIA_BAN, "ID", "TEN_DIA_BAN", DiaBan.PARENTID);
            //ViewBag.PARENTID = new SelectList(db.DIA_BAN.Where(dv => dv.PARENTID == "0"), "MA_DIA_BAN", "TEN_DIA_BAN", DiaBan.PARENTID);
            return View(DiaBan);
        }

        // POST: DiaBan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public ActionResult Edit(DIA_BAN DiaBan)
        {
            if (ModelState.IsValid)
            {
                db.Entry(DiaBan).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.PARENTID = new SelectList(db.DIA_BAN, "ID", "TEN_DIA_BAN", DiaBan.PARENTID);
            //ViewBag.PARENTID = new SelectList(db.DIA_BAN.Where(dv => dv.PARENTID == "0"), "MA_DIA_BAN", "TEN_DIA_BAN", DiaBan.PARENTID);
            return View(DiaBan);
        }

        // GET: DiaBan/Delete/5
        //[Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            var DiaBan = (DIA_BAN)db.DIA_BAN.Where(m => m.ID == id).FirstOrDefault();
            if (DiaBan == null) return HttpNotFound();
            return View(DiaBan);
        }

        // POST: DiaBan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            var DiaBan = (DIA_BAN)db.DIA_BAN.Where(m => m.ID == id).FirstOrDefault();
            db.DIA_BAN.Remove(DiaBan);
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