using QuizStudyAS.DTOs;
using QuizStudyAS.Models;
using QuizStudyAS.ViewModels;

namespace QuizStudyAS.Services.Exam
{
    public interface IExamService
    {
        // Học sinh bắt đầu làm bài
        Task<ServiceResult<ExamAttempt>> StartExamAsync(int examId);

        // Học sinh nộp bài
        Task<ServiceResult<double>> SubmitExamAsync(int attemptId, List<StudentAnswerDTO> studentAnswers);
        Task<ServiceResult<MyExamsVM>> GetMyExamsAsync();
        Task<ServiceResult> CreateExamFromCsvAsync(CreateExamVM model, string ownerUserId);
        Task<ServiceResult<ExamResultVM>> GetExamResultsAsync(int examId, string ownerUserId);
        Task<ServiceResult<ReviewExamVM>> GetExamReviewAsync(int attemptId, string requestUserId);
        Task<int> GetPendingExamsCountAsync(string userId);
    }
}