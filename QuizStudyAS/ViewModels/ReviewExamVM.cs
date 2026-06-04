using System.Collections.Generic;

namespace QuizStudyAS.ViewModels
{
    public class ReviewExamVM
    {
        public string ExamTitle { get; set; }
        public string StudentName { get; set; }
        public double Score { get; set; }
        public List<ReviewQuestionVM> Questions { get; set; } = new List<ReviewQuestionVM>();
    }

    public class ReviewQuestionVM
    {
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public string? Explanation { get; set; }
        public List<ReviewOptionVM> Options { get; set; } = new List<ReviewOptionVM>();
    }

    public class ReviewOptionVM
    {
        public int OptionId { get; set; }
        public string Content { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsSelected { get; set; } // Học sinh có chọn đáp án này không
    }
}