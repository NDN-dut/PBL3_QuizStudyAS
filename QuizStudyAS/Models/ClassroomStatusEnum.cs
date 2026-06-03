namespace QuizStudyAS.Models
{
    public enum ClassroomStatusEnum
    {
        Active = 1,         // Lớp học đang hoạt động
        DeletedByUser = 2,  // Lớp bị giáo viên (Owner) xóa mềm
        LockedByAdmin = 3   // Lớp bị Admin khóa do vi phạm
    }
}