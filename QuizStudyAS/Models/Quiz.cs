namespace QuizStudyAS.Models
{
    public class Quiz
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public virtual User User { get; set; }
    }
}
