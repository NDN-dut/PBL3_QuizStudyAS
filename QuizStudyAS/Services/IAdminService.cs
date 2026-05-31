using QuizStudyAS.Models;
using QuizStudyAS.DTOs; // Thêm thư viện DTO

namespace QuizStudyAS.Services
{
    public interface IAdminService
    {
        // Thống kê cho Dashboard (Giữ nguyên Tuple chứa dữ liệu)
        (int TotalUsers, int TotalStudySets, int TotalClassrooms, int TotalAdmins) GetDashboardStats();

        // Lấy danh sách người dùng (có lọc) và danh sách vai trò
        List<ApplicationUser> GetFilteredUsers(string searchString, int? roleId);
        List<Role> GetAllRoles();

        // Các thao tác CRUD với User
        ApplicationUser? GetUserById(string id);
        ServiceResult AddUser(string userName, string email, string password, int roleId);
        ServiceResult EditUser(string id, string userName, int roleId);
        ServiceResult ToggleUserStatus(string id, string currentUserId);

        // Các thao tác quản lý nội dung (Khóa/Mở khóa)
        ServiceResult ToggleClassroomStatus(int classroomId);
        ServiceResult ToggleStudySetStatus(int studySetId);

        // Lấy danh sách để Admin quản lý (bao gồm cả tìm kiếm)
        List<Classroom> GetFilteredClassrooms(string searchString);
        List<StudySet> GetFilteredStudySets(string searchString);
    }
}