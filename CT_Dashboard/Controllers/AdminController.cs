using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using CT_Dashboard.Models;

namespace CT_Dashboard.Controllers
{
    public class AdminController : Controller
    {
        private readonly CT_DASHBOARDEntities _db = new CT_DASHBOARDEntities();

        // Action để hash lại tất cả mật khẩu trong database
        //Chạy lệnh sau, nhớ sửa port
        //http://localhost:port/Admin/HashAllPasswords
        // CHÚ Ý: Chỉ chạy action này MỘT LẦN DUY NHẤT!
        public ActionResult HashAllPasswords()
        {
            try
            {
                // Lấy tất cả người dùng
                var users = _db.NGUOI_DUNG.ToList();
                int count = 0;

                foreach (var user in users)
                {
                    // Kiểm tra nếu mật khẩu chưa được hash (độ dài SHA256 hash = 64 ký tự)
                    if (user.PASSWORD.Length != 64)
                    {
                        // Lưu mật khẩu cũ (plain text)
                        string oldPassword = user.PASSWORD;

                        // Hash mật khẩu
                        user.PASSWORD = HashPassword(oldPassword);
                        count++;
                    }
                }

                // Lưu thay đổi vào database
                _db.SaveChanges();

                return Content($"Đã hash thành công {count} mật khẩu!");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi: {ex.Message}");
            }
        }

        // Action để hash mật khẩu cho một user cụ thể
        public ActionResult HashPasswordForUser(string username, string newPassword)
        {
            try
            {
                var user = _db.NGUOI_DUNG.FirstOrDefault(u => u.USERNAME == username);

                if (user == null)
                {
                    return Content("Không tìm thấy user!");
                }

                user.PASSWORD = HashPassword(newPassword);
                _db.SaveChanges();

                return Content($"Đã cập nhật mật khẩu cho user: {username}");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi: {ex.Message}");
            }
        }

        // Action để tạo user mới với mật khẩu đã hash
        public ActionResult CreateUserWithHashedPassword(string username, string password, string fullName)
        {
            try
            {
                // Kiểm tra user đã tồn tại chưa
                var existingUser = _db.NGUOI_DUNG.FirstOrDefault(u => u.USERNAME == username);
                if (existingUser != null)
                {
                    return Content("Username đã tồn tại!");
                }

                // Tạo user mới
                var newUser = new NGUOI_DUNG
                {
                    USERNAME = username,
                    PASSWORD = HashPassword(password),
                    // Thêm các trường khác nếu cần
                    // HO_TEN = fullName,
                    // NGAY_TAO = DateTime.Now
                };

                _db.NGUOI_DUNG.Add(newUser);
                _db.SaveChanges();

                return Content($"Đã tạo user mới: {username}");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi: {ex.Message}");
            }
        }

        // Phương thức hash mật khẩu
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

        // Action để test hash
        public ActionResult TestHash(string password)
        {
            string hashed = HashPassword(password);
            return Content($"Mật khẩu: {password}<br/>Hash: {hashed}<br/>Độ dài: {hashed.Length}");
        }
    }
}