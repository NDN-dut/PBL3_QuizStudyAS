using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Models;

namespace QuizStudyAS.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public AdminService(AppDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public (int TotalUsers, int TotalStudySets, int TotalClassrooms, int TotalAdmins) GetDashboardStats()
        {
            return (
                _context.Users.Count(),
                _context.StudySets.Count(),
                _context.Classrooms.Count(),
                _context.Users.Count(u => u.RoleId == 1)
            );
        }

        public List<ApplicationUser> GetFilteredUsers(string searchString, int? roleId)
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.UserName.Contains(searchString) || u.Email.Contains(searchString));
            }

            if (roleId.HasValue && roleId > 0)
            {
                query = query.Where(u => u.RoleId == roleId);
            }

            return query.ToList();
        }

        public List<Role> GetAllRoles()
        {
            return _context.Roles.ToList();
        }

        public ApplicationUser? GetUserById(string id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }

        public (bool Success, string Message) AddUser(string userName, string email, string password, int roleId)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return (false, "Vui lòng nhập đủ Tên tài khoản và Mật khẩu.");

            if (_context.Users.Any(u => u.UserName == userName || u.Email == email))
                return (false, "Tên tài khoản hoặc Email đã tồn tại!");

            var newUser = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(password),
                RoleId = roleId
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return (true, "Thêm người dùng thành công!");
        }

        public (bool Success, string Message) EditUser(string id, string userName, int roleId)
        {
            var user = _context.Users.Find(id);
            if (user == null) return (false, "Không tìm thấy người dùng.");

            user.UserName = userName;
            user.RoleId = roleId;
            _context.SaveChanges();

            return (true, "Cập nhật thông tin thành công.");
        }

        public (bool Success, string Message) ToggleUserStatus(string id, string currentUserId)
        {
            var user = _context.Users.Find(id);
            if (user == null) return (false, "Không tìm thấy người dùng.");

            if (user.Id == currentUserId)
                return (false, "Bạn không thể tự khóa tài khoản của chính mình!");

            user.IsActive = !user.IsActive;
            _context.SaveChanges();

            var msg = user.IsActive ? "Đã mở khóa tài khoản thành công." : "Đã khóa tài khoản thành công.";
            return (true, msg);
        }
    }
}