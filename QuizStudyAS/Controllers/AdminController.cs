using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Thêm thư viện này để dùng Include()
using QuizStudyAS.Attributes;
using QuizStudyAS.Data;

namespace QuizStudyAS.Controllers
{
    [AuthorizeRole("Admin")] // Toàn bộ Controller này được bảo vệ
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        // Dependency Injection: Nhận DbContext từ Program.cs
        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        // --- HÀM MỚI: QUẢN LÝ NGƯỜI DÙNG ---
        public IActionResult ManageUsers()
        {
            // Lấy danh sách Users, đồng thời JOIN với bảng Roles để lấy tên quyền
            var users = _context.Users
                                .Include(u => u.Role)
                                .ToList();

            // Truyền danh sách này sang View
            return View(users);
        }
        // --- LẤY THÔNG TIN NGƯỜI DÙNG ĐỂ HIỆN LÊN MODAL SỬA ---
        [HttpGet]
        public IActionResult GetUser(string id)
        {
            var user = _context.Users.Select(u => new {
                u.Id,
                u.UserName,
                u.Email,
                u.RoleId
            }).FirstOrDefault(u => u.Id == id);

            if (user == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });
            return Json(new { success = true, data = user });
        }

        // --- XỬ LÝ CẬP NHẬT THÔNG TIN ---
        [HttpPost]
        public IActionResult EditUser(string id, string email, int roleId)
        {
            var user = _context.Users.Find(id);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });

            // Cập nhật thông tin
            user.Email = email;
            user.RoleId = roleId;

            _context.SaveChanges();
            return Json(new { success = true, message = "Cập nhật thành công!" });
        }

        // --- XỬ LÝ XÓA NGƯỜI DÙNG ---
        [HttpPost]
        public IActionResult DeleteUser(string id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });

            // Không cho phép tự xóa chính mình (Admin đang đăng nhập)
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (user.Id == currentUserId)
            {
                return Json(new { success = false, message = "Bạn không thể tự xóa tài khoản của chính mình!" });
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return Json(new { success = true, message = "Đã xóa người dùng thành công." });
        }
    }
}