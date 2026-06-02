namespace QuizStudyAS.Models
{
    public class AuthProvider
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // VD: "Local", "Google"

        // Navigation property
        public virtual ICollection<ApplicationUser> Users { get; set; }

        public AuthProvider()
        {
            Users = new HashSet<ApplicationUser>();
        }
    }
}