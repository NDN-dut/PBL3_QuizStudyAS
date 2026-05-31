using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Models;
using QuizStudyAS.DTOs; // Thêm thư viện DTO

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

        public ServiceResult AddUser(string userName, string email, string password, int roleId)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return ServiceResult.IsError("Vui lòng nhập đủ Tên tài khoản và Mật khẩu.");

            if (_context.Users.Any(u => u.UserName == userName || u.Email == email))
                return ServiceResult.IsError("Tên tài khoản hoặc Email đã tồn tại!");

            var newUser = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(password),
                RoleId = roleId
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return ServiceResult.IsSuccess("Thêm người dùng thành công!");
        }

        public ServiceResult EditUser(string id, string userName, int roleId)
        {
            var user = _context.Users.Find(id);
            if (user == null) return ServiceResult.IsError("Không tìm thấy người dùng.");

            user.UserName = userName;
            user.RoleId = roleId;
            _context.SaveChanges();

            return ServiceResult.IsSuccess("Cập nhật thông tin thành công.");
        }

        public ServiceResult ToggleUserStatus(string id, string currentUserId)
        {
            var user = _context.Users.Find(id);
            if (user == null) return ServiceResult.IsError("Không tìm thấy người dùng.");

            if (user.Id == currentUserId)
                return ServiceResult.IsError("Bạn không thể tự khóa tài khoản của chính mình!");

            user.IsActive = !user.IsActive;
            _context.SaveChanges();

            var msg = user.IsActive ? "Đã mở khóa tài khoản thành công." : "Đã khóa tài khoản thành công.";
            return ServiceResult.IsSuccess(msg);
        }

        public ServiceResult ToggleClassroomStatus(int classroomId)
        {
            var classroom = _context.Classrooms.Find(classroomId);
            if (classroom == null)
                return ServiceResult.IsError("Không tìm thấy lớp học.");

            classroom.IsActive = !classroom.IsActive;
            _context.SaveChanges();

            var msg = classroom.IsActive ? "Đã mở khóa lớp học thành công." : "Đã khóa lớp học thành công.";
            return ServiceResult.IsSuccess(msg);
        }

        public ServiceResult ToggleStudySetStatus(int studySetId)
        {
            var studySet = _context.StudySets.Find(studySetId);
            if (studySet == null)
                return ServiceResult.IsError("Không tìm thấy học phần.");

            studySet.IsActive = !studySet.IsActive;
            _context.SaveChanges();

            var msg = studySet.IsActive ? "Đã mở khóa học phần thành công." : "Đã khóa học phần thành công.";
            return ServiceResult.IsSuccess(msg);
        }

        public List<Classroom> GetFilteredClassrooms(string searchString)
        {
            var query = _context.Classrooms.Include(c => c.OwnerUser).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.ClassName.Contains(searchString) || c.InviteCode.Contains(searchString));
            }

            return query.ToList();
        }

        public List<StudySet> GetFilteredStudySets(string searchString)
        {
            var query = _context.StudySets.Include(s => s.OwnerUser).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Title.Contains(searchString));
            }

            return query.ToList();
        }
    }
}