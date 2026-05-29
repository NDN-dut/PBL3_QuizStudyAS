using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Models;

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

        public (bool Success, string Message) RegisterUser(string username, string email, string password)
        {
            if (_context.Users.Any(u => u.UserName == username || u.Email == email))
            {
                return (false, "Tên tài khoản hoặc Email đã được sử dụng.");
            }

            // 1. Tìm quyền "User" trong Database
            var defaultRole = _context.Roles.FirstOrDefault(r => r.RoleName == "User");

            // 2. NẾU CHƯA CÓ -> TỰ ĐỘNG TẠO MỚI LUÔN ĐỂ TRÁNH LỖI KHÓA NGOẠI
            if (defaultRole == null)
            {
                defaultRole = new Role { RoleName = "User" };
                _context.Roles.Add(defaultRole);
                _context.SaveChanges(); // Lưu xuống DB để lấy được RoleId chuẩn xác
            }

            // 3. Tạo tài khoản với RoleId chuẩn xác
            var newUser = new ApplicationUser
            {
                UserName = username,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(password),
                RoleId = defaultRole.RoleId // Lấy ID trực tiếp từ đối tượng Role
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return (true, "Đăng ký thành công! Vui lòng đăng nhập.");
        }
        public (bool Success, ApplicationUser? User, string Message) AuthenticateUser(string usernameOrEmail, string password)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return (false, null, "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            if (!user.IsActive)
            {
                return (false, null, "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ Admin.");
            }

            return (true, user, "Đăng nhập thành công.");
        }

        public ApplicationUser? GeneratePasswordResetToken(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            // Không tạo token cho user không tồn tại hoặc đã bị khóa
            if (user == null || !user.IsActive) return null;

            user.ResetPasswordToken = Guid.NewGuid().ToString();
            user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
            _context.SaveChanges();

            return user;
        }

        public (bool Success, string Message) ResetPassword(string token, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.ResetPasswordToken == token &&
                u.ResetPasswordExpiry > DateTime.Now);

            if (user == null)
            {
                return (false, "Đường dẫn khôi phục không hợp lệ hoặc đã hết thời gian (15 phút).");
            }

            user.PasswordHash = _passwordHasher.HashPassword(newPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;
            _context.SaveChanges();

            return (true, "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập ngay bây giờ.");
        }
    }
}