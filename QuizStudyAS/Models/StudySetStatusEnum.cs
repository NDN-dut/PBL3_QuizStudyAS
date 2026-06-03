namespace QuizStudyAS.Models
{
    public enum StudySetStatusEnum
    {
        Active = 1,         // Đang hoạt động bình thường
        DeletedByUser = 2,  // Người dùng chủ động xóa mềm
        LockedByAdmin = 3   // Bị Admin khóa do vi phạm
    }
}