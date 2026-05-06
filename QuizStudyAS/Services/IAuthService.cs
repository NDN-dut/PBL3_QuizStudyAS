using QuizStudyAS.Models;

namespace QuizStudyAS.Services
{
    public interface IAuthService
    {
        // Sử dụng ValueTuple để trả về nhiều giá trị cùng lúc (Thành công/Thất bại, Thông báo, Dữ liệu)
        (bool Success, string Message) RegisterUser(string username, string email, string password);

        (bool Success, ApplicationUser? User, string Message) AuthenticateUser(string usernameOrEmail, string password);

        ApplicationUser? GeneratePasswordResetToken(string email);

        (bool Success, string Message) ResetPassword(string token, string newPassword);
    }
}