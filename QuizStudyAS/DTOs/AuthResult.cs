using QuizStudyAS.Models;

namespace QuizStudyAS.DTOs
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}
