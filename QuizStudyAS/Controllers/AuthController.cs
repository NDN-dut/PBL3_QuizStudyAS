using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Models;
using QuizStudyAS.Services;
using QuizStudyAS.ViewModels;

namespace QuizStudyAS.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        // Dependency Injection bơm Context và Hasher vào đây
        public AuthController(AppDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // --- GIAO DIỆN ĐĂNG KÝ ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra xem Username hoặc Email đã tồn tại chưa
                bool isExist = _context.Users.Any(u => u.UserName == model.UserName || u.Email == model.Email);
                if (isExist)
                {
                    // Trả về JSON báo lỗi
                    return Json(new { success = false, message = "Tên tài khoản hoặc Email đã được sử dụng." });
                }

                // 2. Tạo đối tượng User mới
                // Tìm ID của quyền "User" trong Database
                var defaultRole = _context.Roles.FirstOrDefault(r => r.RoleName == "User");
                int defaultRoleId = defaultRole != null ? defaultRole.RoleId : 2; // Dự phòng an toàn

                var newUser = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PasswordHash = _passwordHasher.HashPassword(model.Password),
                    RoleId = defaultRoleId // Gán quyền mặc định
                };

                // 3. Lưu vào Database
                _context.Users.Add(newUser);
                _context.SaveChanges();

                // Trả về JSON báo thành công
                return Json(new { success = true, message = "Đăng ký thành công! Vui lòng đăng nhập." });
            }

            // Nếu Validation lỗi (ví dụ chưa nhập đủ trường)
            var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            return Json(new { success = false, message = error ?? "Dữ liệu không hợp lệ." });
        }

        // --- GIAO DIỆN ĐĂNG NHẬP ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                // 1. Tìm User theo Username hoặc Email
                var user = _context.Users
            .Include(u => u.Role)
            .FirstOrDefault(u => u.UserName == model.UserNameOrEmail || u.Email == model.UserNameOrEmail);

                // 2. Kiểm tra sự tồn tại và so khớp mật khẩu
                if (user != null && _passwordHasher.VerifyPassword(model.Password, user.PasswordHash))
                {
                    // --- LƯU THÔNG TIN VÀO SESSION ---
                    HttpContext.Session.SetString("UserName", user.UserName);
                    HttpContext.Session.SetString("UserId", user.Id);
                    HttpContext.Session.SetString("UserRole", user.Role.RoleName);

                    // Trả về JSON báo thành công
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
            }

            var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            return Json(new { success = false, message = error ?? "Dữ liệu không hợp lệ." });
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // Xóa sạch mọi thứ trong Session
            HttpContext.Session.Clear();

            // Quay về trang chủ
            return RedirectToAction("Index", "Home");
        }
    }
}