using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Services;
using QuizStudyAS.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace QuizStudyAS.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

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

        // --- GIAO DIỆN ĐĂNG NHẬP THƯỜNG ---
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
                HttpContext.Session.SetString("UserAvatar", result.User.AvatarUrl ?? "");

                // ĐĂNG NHẬP THƯỜNG: Tên hiển thị chính là UserName gốc của họ
                HttpContext.Session.SetString("UserDisplayName", result.User.UserName);

                return Json(new { success = true });
            }

            return Json(new { success = false, message = result.Message });
        }

        // --- ĐĂNG NHẬP BẰNG GOOGLE ---
        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                ViewBag.ErrorMessage = "Lỗi xác thực từ Google.";
                return View("Login");
            }

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var fullName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (email == null)
            {
                ViewBag.ErrorMessage = "Không thể lấy được email từ tài khoản Google.";
                return View("Login");
            }

            var authResult = await _authService.AuthenticateGoogleUserAsync(email, fullName ?? "");

            if (!authResult.Success || authResult.User == null)
            {
                await HttpContext.SignOutAsync();
                ViewBag.ErrorMessage = authResult.Message;
                return View("Login");
            }

            // Tạo Cookie xác thực để hệ thống nhận diện Middleware
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, authResult.User.UserName),
                new Claim(ClaimTypes.Email, authResult.User.Email),
                new Claim(ClaimTypes.NameIdentifier, authResult.User.Id.ToString()),
                new Claim(ClaimTypes.Role, authResult.User.Role?.RoleName ?? "User")
            };

            var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Đồng bộ dữ liệu vào Session hệ thống giống luồng đăng nhập thường của nhóm
            HttpContext.Session.SetString("UserName", authResult.User.UserName);
            HttpContext.Session.SetString("UserId", authResult.User.Id);
            HttpContext.Session.SetString("UserRole", authResult.User.Role?.RoleName ?? "User");
            HttpContext.Session.SetInt32("UserLevel", authResult.User.Level);
            HttpContext.Session.SetInt32("UserStreak", authResult.User.CurrentStreak);
            HttpContext.Session.SetString("UserAvatar", authResult.User.AvatarUrl ?? "");

            // ĐĂNG NHẬP GOOGLE: Lấy trực tiếp Tên gốc trên Google hiển thị lên Profile
            HttpContext.Session.SetString("UserDisplayName", !string.IsNullOrEmpty(fullName) ? fullName : authResult.User.UserName);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            // Xóa triệt để cả phiên đăng nhập Cookie của ứng dụng lẫn Google
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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