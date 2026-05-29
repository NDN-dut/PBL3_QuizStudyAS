using QuizStudyAS.Models;

namespace QuizStudyAS.ViewModels
{
    // Đại diện cho một Khung chứa (Ví dụ: Khung "Cá nhân", Khung "Lớp A"...)
    public class StudySetGroupVM
    {
        public int ClassroomId { get; set; }
        public string GroupName { get; set; }
        public List<StudySet> StudySets { get; set; } = new List<StudySet>();
    }

    // Gói toàn bộ dữ liệu để gửi ra View
    public class StudySetInventoryVM
    {
        public StudySetGroupVM PersonalGroup { get; set; } = new StudySetGroupVM();
        public List<StudySetGroupVM> ClassGroups { get; set; } = new List<StudySetGroupVM>();
    }
}