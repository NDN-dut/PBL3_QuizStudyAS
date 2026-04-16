namespace QuizStudyAS.Models
{
    public class User
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public virtual ICollection<Quiz> Quizzes { get; set; }
        public User()
        {
            Quizzes = new HashSet<Quiz>();
        }
    }
}
