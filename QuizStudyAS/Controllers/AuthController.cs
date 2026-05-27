using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Services;
using QuizStudyAS.ViewModels;


namespace QuizStudyAS.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        // DBContext và Hasher đã biến mất khỏi Controller!
        public AuthController(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        // --- GIAO DIỆN ĐĂNG KÝ ---
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return Json(new { success = false, message = error ?? "Dữ liệu không hợp lệ." });
            }

            var result = _authService.RegisterUser(model.UserName, model.Email, model.Password);
            return Json(new { success = result.Success, message = result.Message });
        }

        // --- GIAO DIỆN ĐĂNG NHẬP ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return Json(new { success = false, message = error ?? "Dữ liệu không hợp lệ." });
            }

            var result = _authService.AuthenticateUser(model.UserNameOrEmail, model.Password);

            if (result.Success && result.User != null)
            {
                HttpContext.Session.SetString("UserName", result.User.UserName);
                HttpContext.Session.SetString("UserId", result.User.Id);
                HttpContext.Session.SetString("UserRole", result.User.Role.RoleName);

                HttpContext.Session.SetInt32("UserLevel", result.User.Level);
                HttpContext.Session.SetInt32("UserStreak", result.User.CurrentStreak);

                return Json(new { success = true });
            }

            return Json(new { success = false, message = result.Message });
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login1() => RedirectToAction("Index", "Home");

        // --- QUÊN MẬT KHẨU ---
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _authService.GeneratePasswordResetToken(model.Email);

            if (user != null)
            {
                var resetLink = Url.Action("ResetPassword", "Auth", new { token = user.ResetPasswordToken }, Request.Scheme);

                var subject = "Yêu cầu khôi phục mật khẩu - QSAS";
                var body = $@"
                    <h3>Xin chào {user.UserName},</h3>
                    <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản tại QSAS.</p>
                    <p>Vui lòng click vào nút bên dưới để tiến hành đổi mật khẩu. Đường link này chỉ có hiệu lực trong vòng <strong>15 phút</strong>.</p>
                    <a href='{resetLink}' style='display:inline-block; padding:10px 20px; background-color:#198754; color:white; text-decoration:none; border-radius:5px;'>ĐẶT LẠI MẬT KHẨU</a>
                    <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            ViewBag.Message = "Nếu email hợp lệ và tài khoản đang hoạt động, một đường link khôi phục đã được gửi vào hòm thư của bạn.";
            return View();
        }

        // --- ĐẶT LẠI MẬT KHẨU ---
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Index", "Home");
            return View(new ResetPasswordVM { Token = token });
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = _authService.ResetPassword(model.Token, model.NewPassword);

            if (!result.Success)
            {
                ViewBag.ErrorMessage = result.Message;
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("Index", "Home");
        }
    }
}