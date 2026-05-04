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
        private readonly IEmailService _emailService;

        // Dependency Injection bơm Context và Hasher vào đây
        public AuthController(AppDbContext context, IPasswordHasher passwordHasher, IEmailService emailService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
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
        [HttpGet]
        public IActionResult Login1()
        {
            // Tạm thời đẩy về trang chủ để hiện Modal đăng nhập
            return RedirectToAction("Index", "Home");
        }

        // --- HÀM GET: HIỂN THỊ FORM QUÊN MẬT KHẨU ---
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // --- HÀM POST: XỬ LÝ GỬI MAIL ---
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Tìm User theo Email
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user != null)
            {
                // THÊM ĐOẠN NÀY: Kiểm tra xem tài khoản có bị khóa không
                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ Admin.");
                    return View(model);
                }

                // 2. Sinh Token ngẫu nhiên và đặt thời hạn (15 phút)
                user.ResetPasswordToken = Guid.NewGuid().ToString();
                user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
                _context.SaveChanges();

                // 3. Tạo đường Link khôi phục
                // Url.Action giúp tự động build ra link dạng: https://localhost:7235/Auth/ResetPassword?token=abc-123
                var resetLink = Url.Action("ResetPassword", "Auth", new { token = user.ResetPasswordToken }, Request.Scheme);

                // 4. Gửi Email
                var subject = "Yêu cầu khôi phục mật khẩu - QSAS";
                var body = $@"
                    <h3>Xin chào {user.UserName},</h3>
                    <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản tại QSAS.</p>
                    <p>Vui lòng click vào nút bên dưới để tiến hành đổi mật khẩu. Đường link này chỉ có hiệu lực trong vòng <strong>15 phút</strong>.</p>
                    <a href='{resetLink}' style='display:inline-block; padding:10px 20px; background-color:#198754; color:white; text-decoration:none; border-radius:5px;'>ĐẶT LẠI MẬT KHẨU</a>
                    <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            // 5. Trả về thông báo (Dù email có tồn tại hay không, vẫn báo chung một câu để tránh hacker dò quét email trong hệ thống)
            ViewBag.Message = "Nếu email hợp lệ, một đường link khôi phục đã được gửi vào hòm thư của bạn.";
            return View();
        }

        // --- HÀM GET: HIỂN THỊ FORM ĐẶT LẠI MẬT KHẨU ---
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            // Nếu ai đó cố tình vào trang này mà không có mã token trên thanh URL thì đuổi về trang chủ
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home");
            }

            // Đổ token vào ViewModel để đẩy ra View
            var model = new ResetPasswordVM { Token = token };
            return View(model);
        }

        // --- HÀM POST: XỬ LÝ ĐẶT LẠI MẬT KHẨU ---
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Quét trong DB xem có User nào khớp Token và thời gian còn hạn không
            var user = _context.Users.FirstOrDefault(u =>
                u.ResetPasswordToken == model.Token &&
                u.ResetPasswordExpiry > DateTime.Now);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Đường dẫn khôi phục không hợp lệ hoặc đã hết thời gian (15 phút). Vui lòng yêu cầu gửi lại email.";
                return View(model);
            }

            // 2. Nếu hợp lệ: Băm mật khẩu mới và lưu đè vào DB
            user.PasswordHash = _passwordHasher.HashPassword(model.NewPassword);

            // 3. Quan trọng: Xóa trắng Token để Link trong email trở thành "đồ bỏ đi", tránh bị dùng lại
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;

            _context.SaveChanges();

            // 4. Báo thành công và điều hướng
            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập bằng mật khẩu mới ngay bây giờ.";
            return RedirectToAction("Index", "Home");
        }
    }

}
