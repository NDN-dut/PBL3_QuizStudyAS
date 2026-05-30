using QuizStudyAS.Models;

namespace QuizStudyAS.Services
{
    public interface IAdminService
    {
        // Thống kê cho Dashboard
        (int TotalUsers, int TotalStudySets, int TotalClassrooms, int TotalAdmins) GetDashboardStats();

        // Lấy danh sách người dùng (có lọc) và danh sách vai trò
        List<ApplicationUser> GetFilteredUsers(string searchString, int? roleId);
        List<Role> GetAllRoles();

        // Các thao tác CRUD với User
        ApplicationUser? GetUserById(string id);
        (bool Success, string Message) AddUser(string userName, string email, string password, int roleId);
        (bool Success, string Message) EditUser(string id, string userName, int roleId);
        (bool Success, string Message) ToggleUserStatus(string id, string currentUserId);
        // Các thao tác quản lý nội dung (Khóa/Mở khóa)
        (bool Success, string Message) ToggleClassroomStatus(int classroomId);
        (bool Success, string Message) ToggleStudySetStatus(int studySetId);
        // Lấy danh sách để Admin quản lý (bao gồm cả tìm kiếm)
        List<Classroom> GetFilteredClassrooms(string searchString);
        List<StudySet> GetFilteredStudySets(string searchString);
    }
}