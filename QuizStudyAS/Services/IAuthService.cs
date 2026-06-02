using QuizStudyAS.Models;
using QuizStudyAS.DTOs;
using System.Threading.Tasks;

namespace QuizStudyAS.Services
{
    public interface IAuthService
    {
        ServiceResult RegisterUser(string username, string email, string password);

        ServiceResult<ApplicationUser> AuthenticateUser(string usernameOrEmail, string password);

        ServiceResult<ApplicationUser> GeneratePasswordResetToken(string email);

        ServiceResult ResetPassword(string token, string newPassword);

        Task<ServiceResult<ApplicationUser>> AuthenticateGoogleUserAsync(string email, string fullName);
    }
}