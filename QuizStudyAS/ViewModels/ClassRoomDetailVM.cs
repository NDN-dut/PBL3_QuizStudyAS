namespace QuizStudyAS.ViewModels
{
    public class UserInfo{
        public string UserName { get; set; }
        public string UserId { get; set; }
    }
    public class ClassRoomDetailVM
    {
        public string ClassName { get; set; }
        public string OwnerName { get; set; }
        public string ClassCode { get; set; }
        // THÊM DÒNG NÀY: Để mang trạng thái từ DB lên Controller
        public bool IsActive { get; set; }
        public List<StudySetItemVM> StudySets { get; set; }
        public string? ExistingAvatarUrl { get; set; }
        public List<UserInfo> ClassUsers { get; set; }
    }
}
