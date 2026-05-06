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
    }
}