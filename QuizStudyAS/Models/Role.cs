namespace QuizStudyAS.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } // Ví dụ: "Admin", "User"

        // Thuộc tính điều hướng: 1 Role có nhiều User
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
