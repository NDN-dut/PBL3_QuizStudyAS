using QuizStudyAS.Services;

namespace QuizStudyAS.DTOs
{
    public class SubmitExamRequestDTO
    {
        public int AttemptId { get; set; }
        public List<StudentAnswerDTO> Answers { get; set; }
    }
}