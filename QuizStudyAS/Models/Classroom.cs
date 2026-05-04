using QuizStudyAS.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Classroom
{
    [Key]
    public Guid ClassroomId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ClassName { get; set; }

    [Required]
    [MaxLength(10)]
    public string InviteCode { get; set; }

    [Required]
    public Guid OwnerUserId { get; set; }

    // Navigation
    [ForeignKey("OwnerUserId")]
    public virtual ApplicationUser OwnerUser { get; set; }

    public virtual ICollection<ClassroomUser> ClassroomUsers { get; set; }
    public virtual ICollection<StudySet> StudySets { get; set; }

    public Classroom()
    {
        ClassroomId = Guid.NewGuid();
        ClassroomUsers = new HashSet<ClassroomUser>();
        StudySets = new HashSet<StudySet>();
    }
}
