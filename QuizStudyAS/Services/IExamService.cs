using QuizStudyAS.DTOs;
using QuizStudyAS.Models;

namespace QuizStudyAS.Services
{
    // DTO để nhận dữ liệu từ View khi học sinh nộp bài
    public class StudentAnswerDTO
    {
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; }
    }

    public interface IExamService
    {
        // Học sinh bắt đầu làm bài
        Task<ServiceResult<ExamAttempt>> StartExamAsync(int examId);

        // Học sinh nộp bài
        Task<ServiceResult<double>> SubmitExamAsync(int attemptId, List<StudentAnswerDTO> studentAnswers);
    }
}