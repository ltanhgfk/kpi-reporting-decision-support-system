using CT_Dashboard.Filters;
using CT_Dashboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace CT_Dashboard.Controllers
{
    [AuthorizeRole("Admin")]
    public class UserController : Controller
    {
        private readonly CT_DASHBOARDEntities _db = new CT_DASHBOARDEntities();

        // Hàm mã hóa mật khẩu bằng SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public ActionResult Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var users = _db.NGUOI_DUNG.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(c => c.USERNAME.Contains(searchString) || c.HO_TEN.Contains(searchString));
                ViewBag.SearchString = searchString;
            }

            users = users.OrderBy(c => c.DON_VI).ThenBy(c => c.HO_TEN);
            return View(users.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Create()
        {
            ViewBag.DonViList = new SelectList(_db.DON_VI, "MA_DON_VI", "TEN_DON_VI");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NGUOI_DUNG model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra username đã tồn tại chưa
                if (_db.NGUOI_DUNG.Any(u => u.USERNAME == model.USERNAME))
                {
                    ModelState.AddModelError("USERNAME", "Tên đăng nhập đã tồn tại.");
                    ViewBag.DonViList = new SelectList(_db.DON_VI, "MA_DON_VI", "TEN_DON_VI");
                    return View(model);
                }

                // Mã hóa mật khẩu
                model.PASSWORD = HashPassword(model.PASSWORD);

                // Gán thông tin tạo
                model.NGAY_TAO = DateTime.Now;
                model.NGUOI_TAO = (Session["User"] as NGUOI_DUNG)?.ID;

                _db.NGUOI_DUNG.Add(model);
                _db.SaveChanges();

                TempData["SuccessMessage"] = "Thêm người dùng thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.DonViList = new SelectList(_db.DON_VI, "MA_DON_VI", "TEN_DON_VI");
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var user = _db.NGUOI_DUNG.Find(id);
            if (user == null) return HttpNotFound();

            ViewBag.DonViList = new SelectList(_db.DON_VI, "MA_DON_VI", "TEN_DON_VI", user.DON_VI);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NGUOI_DUNG model, string NewPassword)
        {
            if (ModelState.IsValid)
            {
                var user = _db.NGUOI_DUNG.Find(model.ID);
                if (user == null) return HttpNotFound();

                // Cập nhật thông tin
                user.HO_TEN = model.HO_TEN;
                user.DIEN_THOAI = model.DIEN_THOAI;
                user.EMAIL = model.EMAIL;
                user.DON_VI = model.DON_VI;
                user.TRANG_THAI = model.TRANG_THAI;
                user.VAI_TRO = model.VAI_TRO;

                // Chỉ cập nhật mật khẩu nếu người dùng nhập mật khẩu mới
                if (!string.IsNullOrEmpty(NewPassword))
                {
                    user.PASSWORD = HashPassword(NewPassword);
                }

                // Gán thông tin sửa
                user.NGAY_SUA = DateTime.Now;
                user.NGUOI_SUA = (Session["User"] as NGUOI_DUNG)?.ID;

                _db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.DonViList = new SelectList(_db.DON_VI, "MA_DON_VI", "TEN_DON_VI", model.DON_VI);
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var user = _db.NGUOI_DUNG.Find(id);
            if (user == null) return HttpNotFound();

            // Không cho phép xóa chính mình
            var currentUser = Session["User"] as NGUOI_DUNG;
            if (currentUser != null && currentUser.ID == id)
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản đang đăng nhập!";
                return RedirectToAction("Index");
            }

            _db.NGUOI_DUNG.Remove(user);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Xóa người dùng thành công!";
            return RedirectToAction("Index");
        }
    }
}