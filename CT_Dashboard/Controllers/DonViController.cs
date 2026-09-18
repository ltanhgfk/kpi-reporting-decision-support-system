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
    public class DonViController : Controller
    {
        private CT_DASHBOARDEntities db = new CT_DASHBOARDEntities();

        // GET: DonVi
        public ActionResult Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var donVis = db.DON_VI.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                donVis = donVis.Where(d => d.MA_DON_VI.Contains(searchString) || d.TEN_DON_VI.Contains(searchString));
                ViewBag.SearchString = searchString;
            }
            //donVis = donVis.OrderBy(d => d.PARENTID).ThenBy(d => d.VI_TRI);
            donVis = donVis.OrderBy(d => d.VI_TRI);
            return View(donVis.ToPagedList(pageNumber, pageSize));
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
                        var worksheet = package.Workbook.Worksheets["DonVi"];
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {                           
                            var donVi = new DON_VI
                            {
                                MA_DON_VI = worksheet.Cells[row, 2].Value?.ToString(), //donViId,
                                TEN_DON_VI = worksheet.Cells[row, 3].Value?.ToString(),//chiTieuId,
                                VI_TRI = byte.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out byte vitri) ? vitri : (byte)1,
                            };

                            db.DON_VI.Add(donVi);
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


        // GET: DonVi/Create
        //[Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            //ViewBag.PARENTID = new SelectList(db.DON_VI, "ID", "TEN_DON_VI");
           // ViewBag.PARENTID = new SelectList(db.DON_VI.Where(dv => dv.PARENTID == "0"), "MA_DON_VI", "TEN_DON_VI");
            return View();
        }

        // POST: DonVi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public ActionResult Create(DON_VI donVi)
        {
            if (ModelState.IsValid)
            {
                db.DON_VI.Add(donVi);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.PARENTID = new SelectList(db.DON_VI, "ID", "TEN_DON_VI", donVi.PARENTID);
            //ViewBag.PARENTID = new SelectList(db.DON_VI.Where(dv => dv.PARENTID == "0"), "MA_DON_VI", "TEN_DON_VI", donVi.PARENTID);
            return View(donVi);
        }

        // GET: DonVi/Edit/5
        //[Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var donVi = (DON_VI)db.DON_VI.Where(m => m.ID == id).FirstOrDefault();
            if (donVi == null) return HttpNotFound();
            //ViewBag.PARENTID = new SelectList(db.DON_VI, "ID", "TEN_DON_VI", donVi.PARENTID);
            //ViewBag.PARENTID = new SelectList(db.DON_VI.Where(dv => dv.PARENTID == "0"), "MA_DON_VI", "TEN_DON_VI", donVi.PARENTID);
            return View(donVi);
        }

        // POST: DonVi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public ActionResult Edit(DON_VI donVi)
        {
            if (ModelState.IsValid)
            {
                db.Entry(donVi).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.PARENTID = new SelectList(db.DON_VI, "ID", "TEN_DON_VI", donVi.PARENTID);
            //ViewBag.PARENTID = new SelectList(db.DON_VI.Where(dv => dv.PARENTID == "0"), "MA_DON_VI", "TEN_DON_VI", donVi.PARENTID);
            return View(donVi);
        }

        // GET: DonVi/Delete/5
        //[Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            var donVi = (DON_VI)db.DON_VI.Where(m => m.ID == id).FirstOrDefault();
            if (donVi == null) return HttpNotFound();
            return View(donVi);
        }

        // POST: DonVi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            var donVi = (DON_VI)db.DON_VI.Where(m => m.ID == id).FirstOrDefault();
            db.DON_VI.Remove(donVi);
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