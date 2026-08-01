using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Models;
using QuizStudyAS.DTOs; // Thêm thư viện DTO
using QuizStudyAS.Services.Auth;
namespace QuizStudyAS.Services.Admin
{
    using StudySet = QuizStudyAS.Models.StudySet;
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

        public PaginatedList<ApplicationUser> GetFilteredUsers(string searchString, int? roleId, bool? isActive, DateTime? fromDate, DateTime? toDate, int pageIndex = 1, int pageSize = 10)
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(u => u.UserName.Contains(searchString) || u.Email.Contains(searchString));

            if (roleId.HasValue && roleId > 0)
                query = query.Where(u => u.RoleId == roleId);

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            if (fromDate.HasValue)
                query = query.Where(u => u.CreatedAt >= fromDate.Value.Date);

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(u => u.CreatedAt <= endOfDay);
            }

            // Sắp xếp trước khi phân trang (RẤT QUAN TRỌNG: EF Core bắt buộc phải OrderBy trước khi dùng Skip/Take)
            query = query.OrderByDescending(u => u.CreatedAt);

            // Đóng gói vào class PaginatedList
            return PaginatedList<ApplicationUser>.Create(query, pageIndex, pageSize);
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
            // Bổ sung IgnoreQueryFilters()
            var classroom = _context.Classrooms.IgnoreQueryFilters().FirstOrDefault(c => c.ClassroomId == classroomId);
            if (classroom == null)
                return ServiceResult.IsError("Không tìm thấy lớp học.");

            // Tránh Admin thao tác trên lớp đã bị xóa
            if (classroom.StatusId == (int)ClassroomStatusEnum.DeletedByUser)
                return ServiceResult.IsError("Lớp học này đã bị giáo viên xóa, không thể thay đổi trạng thái.");

            bool isCurrentlyActive = classroom.StatusId == (int)ClassroomStatusEnum.Active;

            // Đổi qua lại giữa Active (1) và LockedByAdmin (3)
            classroom.StatusId = isCurrentlyActive ? (int)ClassroomStatusEnum.LockedByAdmin : (int)ClassroomStatusEnum.Active;
            _context.SaveChanges();

            var msg = !isCurrentlyActive ? "Đã mở khóa lớp học thành công." : "Đã khóa lớp học thành công.";
            return ServiceResult.IsSuccess(msg);
        }

        public ServiceResult ToggleStudySetStatus(int studySetId)
        {
            // Bổ sung IgnoreQueryFilters() để Admin có thể tìm thấy cả học phần đã ẩn
            var studySet = _context.StudySets.IgnoreQueryFilters().FirstOrDefault(s => s.StudySetId == studySetId);
            if (studySet == null)
                return ServiceResult.IsError("Không tìm thấy học phần.");

            // Tránh việc Admin thao tác nhầm lên học phần mà User đã tự xóa
            if (studySet.StatusId == (int)StudySetStatusEnum.DeletedByUser)
                return ServiceResult.IsError("Học phần này đã bị người dùng xóa, không thể thay đổi trạng thái.");

            bool isCurrentlyActive = studySet.StatusId == (int)StudySetStatusEnum.Active;

            // Chuyển đổi qua lại giữa Trạng thái Hoạt động (1) và Bị khóa (3)
            studySet.StatusId = isCurrentlyActive ? (int)StudySetStatusEnum.LockedByAdmin : (int)StudySetStatusEnum.Active;
            _context.SaveChanges();

            var msg = !isCurrentlyActive ? "Đã mở khóa học phần thành công." : "Đã khóa học phần thành công.";
            return ServiceResult.IsSuccess(msg);
        }

        public PaginatedList<Classroom> GetFilteredClassrooms(string searchString, int? statusId, string? ownerName, DateTime? fromDate, DateTime? toDate, int pageIndex = 1, int pageSize = 10)
        {
            // THÊM IgnoreQueryFilters() ĐỂ ADMIN THẤY MỌI TRẠNG THÁI
            var query = _context.Classrooms.IgnoreQueryFilters().Include(c => c.OwnerUser).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(c => c.ClassName.Contains(searchString) || c.InviteCode.Contains(searchString));

            if (!string.IsNullOrEmpty(ownerName))
                query = query.Where(c => c.OwnerUser.UserName.Contains(ownerName));

            // THAY ĐỔI CÁCH LỌC BẰNG STATUSID
            if (statusId.HasValue)
                query = query.Where(c => c.StatusId == statusId.Value);

            if (fromDate.HasValue)
                query = query.Where(c => c.CreatedAt >= fromDate.Value.Date);

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(c => c.CreatedAt <= endOfDay);
            }

            query = query.OrderByDescending(c => c.CreatedAt);

            return PaginatedList<Classroom>.Create(query, pageIndex, pageSize);
        }

        public PaginatedList<StudySet> GetFilteredStudySets(string searchString, int? statusId, string? ownerName, DateTime? fromDate, DateTime? toDate, int pageIndex = 1, int pageSize = 10)
        {
            // BẮT BUỘC DÙNG IgnoreQueryFilters() ĐỂ ADMIN CÓ THỂ QUẢN LÝ TOÀN BỘ
            var query = _context.StudySets.IgnoreQueryFilters().Include(s => s.OwnerUser).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(s => s.Title.Contains(searchString));

            if (!string.IsNullOrEmpty(ownerName))
                query = query.Where(s => s.OwnerUser.UserName.Contains(ownerName));

            // THAY ĐỔI LOGIC LỌC: So sánh trực tiếp với StatusId
            if (statusId.HasValue)
            {
                query = query.Where(s => s.StatusId == statusId.Value);
            }

            if (fromDate.HasValue)
                query = query.Where(s => s.CreatedAt >= fromDate.Value.Date);

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(s => s.CreatedAt <= endOfDay);
            }

            query = query.OrderByDescending(s => s.CreatedAt);

            return PaginatedList<StudySet>.Create(query, pageIndex, pageSize);
        }
    }
}