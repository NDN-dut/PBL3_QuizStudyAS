namespace QuizStudyAS.Models
{
    public class Classroom
    {
        public int ClassroomId { get; set; }
        public string ClassName { get; set; }
        public string InviteCode { get; set; } // Mã để học sinh nhập vào tham gia lớp
        public string OwnerUserId { get; set; } // ID của người tạo lớp (ApplicationUser)
        //public bool IsActive { get; set; } = true;
        // --- THÊM KHÓA NGOẠI TRẠNG THÁI ---
        public int StatusId { get; set; } = (int)ClassroomStatusEnum.Active;

        // --- THÊM 2 DÒNG NÀY ---
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // --- Navigation Properties ---
        public virtual ApplicationUser OwnerUser { get; set; }
        public virtual ClassroomStatus Status { get; set; } // Liên kết đến bảng trạng thái
        public virtual ICollection<ClassroomUser> ClassroomUsers { get; set; }
        public virtual ICollection<RequestJoinClass> JoinRequests {  get; set; }
        public virtual ICollection<ClassRoomMaterial> Materials { get; set; }
        public Classroom()
        {
            ClassroomUsers = new HashSet<ClassroomUser>();
            JoinRequests = new HashSet<RequestJoinClass>();
            Materials  = new HashSet<ClassRoomMaterial>();
        }
    }
}