using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.DTOs;
using QuizStudyAS.Models;
using System.Threading.Tasks;

namespace QuizStudyAS.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(AppDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public ServiceResult RegisterUser(string username, string email, string password)
        {
            if (_context.Users.Any(u => u.UserName == username || u.Email == email))
            {
                return ServiceResult.IsError("Tên tài khoản hoặc Email đã được sử dụng.");
            }

            var defaultRole = _context.Roles.FirstOrDefault(r => r.RoleName == "User");
            if (defaultRole == null)
            {
                defaultRole = new Role { RoleName = "User" };
                _context.Roles.Add(defaultRole);
                _context.SaveChanges();
            }

            var newUser = new ApplicationUser
            {
                UserName = username,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(password),
                RoleId = defaultRole.RoleId
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return ServiceResult.IsSuccess("Đăng ký thành công! Vui lòng đăng nhập.");
        }

        public ServiceResult<ApplicationUser> AuthenticateUser(string usernameOrEmail, string password)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null)
            {
                return ServiceResult<ApplicationUser>.IsError("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return ServiceResult<ApplicationUser>.IsError("Tài khoản này được liên kết với Google. Vui lòng chọn 'Đăng nhập bằng Google'.");
            }

            if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return ServiceResult<ApplicationUser>.IsError("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            if (!user.IsActive)
            {
                return ServiceResult<ApplicationUser>.IsError("Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ Admin.");
            }

            return ServiceResult<ApplicationUser>.IsSuccess(user, "Đăng nhập thành công.");
        }

        public ServiceResult<ApplicationUser> GeneratePasswordResetToken(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null || !user.IsActive)
                return ServiceResult<ApplicationUser>.IsError("Email không tồn tại hoặc tài khoản đã bị khóa.");

            user.ResetPasswordToken = Guid.NewGuid().ToString();
            user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
            _context.SaveChanges();

            return ServiceResult<ApplicationUser>.IsSuccess(user);
        }

        public ServiceResult ResetPassword(string token, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.ResetPasswordToken == token &&
                u.ResetPasswordExpiry > DateTime.Now);

            if (user == null)
            {
                return ServiceResult.IsError("Đường dẫn khôi phục không hợp lệ hoặc đã hết thời gian (15 phút).");
            }

            user.PasswordHash = _passwordHasher.HashPassword(newPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;
            _context.SaveChanges();

            return ServiceResult.IsSuccess("Đặt lại mật khẩu thành công! Bạn có thể đăng nhập ngay bây giờ.");
        }

        public async Task<ServiceResult<ApplicationUser>> AuthenticateGoogleUserAsync(string email, string fullName)
        {
            var user = await _context.Users
                         .Include(u => u.Role)
                         .FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                if (!user.IsActive)
                {
                    return ServiceResult<ApplicationUser>.IsError("Tài khoản của bạn đã bị vô hiệu hóa bởi Admin.");
                }
                return ServiceResult<ApplicationUser>.IsSuccess(user, "Đăng nhập qua Google thành công.");
            }

            var googleProvider = await _context.AuthProviders.FirstOrDefaultAsync(p => p.Name == "Google");
            if (googleProvider == null)
            {
                googleProvider = new AuthProvider { Name = "Google" };
                _context.AuthProviders.Add(googleProvider);
            }

            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            if (defaultRole == null)
            {
                defaultRole = new Role { RoleName = "User" };
                _context.Roles.Add(defaultRole);
            }

            var newUser = new ApplicationUser
            {
                UserName = email.Split('@')[0] + "_" + Guid.NewGuid().ToString().Substring(0, 4),
                Email = email,
                PasswordHash = null,
                IsActive = true,
                CreatedAt = DateTime.Now,
                Role = defaultRole,
                AuthProvider = googleProvider
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return ServiceResult<ApplicationUser>.IsSuccess(newUser, "Tạo tài khoản liên kết Google thành công.");
        }
    }
}