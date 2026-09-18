using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CT_Dashboard.Models;

namespace CT_Dashboard.Controllers
{
    public class AccountController : Controller
    {
        private readonly CT_DASHBOARDEntities _db = new CT_DASHBOARDEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Mã hóa mật khẩu người dùng nhập vào
            string hashedPassword = HashPassword(password);

            // Kiểm tra user với mật khẩu đã được hash
            var user = _db.NGUOI_DUNG
                .FirstOrDefault(u => u.USERNAME == username && u.PASSWORD == hashedPassword);

            if (user != null)
            {
                Session["User"] = user;
                return RedirectToAction("Dashboard", "Dashboard");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng.";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // Phương thức mã hóa mật khẩu bằng SHA256
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
    }
}