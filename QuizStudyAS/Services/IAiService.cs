using System.Threading.Tasks;
using QuizStudyAS.DTOs;

namespace QuizStudyAS.Services
{
    public interface IAiService
    {
        // Nhận vào câu hỏi và trả về kết quả được bọc trong ServiceResult
        Task<ServiceResult<string>> GetAnswerAsync(string userMessage);
    }
}