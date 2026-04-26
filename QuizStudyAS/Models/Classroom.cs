namespace QuizStudyAS.Models
{
    public class Classroom
    {
        public int ClassroomId { get; set; }
        public string ClassName { get; set; }
        public string InviteCode { get; set; } // Mã để học sinh nhập vào tham gia lớp
        public string OwnerUserId { get; set; } // ID của người tạo lớp (ApplicationUser)

        // --- Navigation Properties ---
        public virtual ApplicationUser OwnerUser { get; set; }
        public virtual ICollection<ClassroomUser> ClassroomUsers { get; set; }
        public virtual ICollection<StudySet> StudySets { get; set; }
        public virtual ICollection<RequestJoinClass> JoinRequests {  get; set; }
        public Classroom()
        {
            ClassroomUsers = new HashSet<ClassroomUser>();
            StudySets = new HashSet<StudySet>();
            JoinRequests = new HashSet<RequestJoinClass>();
        }
    }
}