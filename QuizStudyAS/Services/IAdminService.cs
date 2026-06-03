using QuizStudyAS.Models;
using QuizStudyAS.DTOs; // Thêm thư viện DTO

namespace QuizStudyAS.Services
{
    public interface IAdminService
    {
        // Thống kê cho Dashboard (Giữ nguyên Tuple chứa dữ liệu)
        (int TotalUsers, int TotalStudySets, int TotalClassrooms, int TotalAdmins) GetDashboardStats();

        // Lấy danh sách người dùng(có lọc và phân trang)
        PaginatedList<ApplicationUser> GetFilteredUsers(string searchString, int? roleId, bool? isActive, DateTime? fromDate, DateTime? toDate, int pageIndex = 1, int pageSize = 10);
        List<Role> GetAllRoles();

        // Các thao tác CRUD với User
        ApplicationUser? GetUserById(string id);
        ServiceResult AddUser(string userName, string email, string password, int roleId);
        ServiceResult EditUser(string id, string userName, int roleId);
        ServiceResult ToggleUserStatus(string id, string currentUserId);

        // Các thao tác quản lý nội dung (Khóa/Mở khóa)
        ServiceResult ToggleClassroomStatus(int classroomId);
        ServiceResult ToggleStudySetStatus(int studySetId);

        // Lấy danh sách Lớp học (có lọc và phân trang)
        PaginatedList<Classroom> GetFilteredClassrooms(string searchString, bool? isActive, string? ownerName, DateTime? fromDate, DateTime? toDate, int pageIndex = 1, int pageSize = 10);
        // Lấy danh sách Học phần (có lọc và phân trang)
        PaginatedList<StudySet> GetFilteredStudySets(string searchString, int? statusId, string? ownerName, DateTime? fromDate, DateTime? toDate, int pageIndex = 1, int pageSize = 10);
    }
}