namespace QuizStudyAS.ViewModels
{
    public class ClassRoomDetailVM
    {
        public string ClassName { get; set; }
        public string OwnerName { get; set; }
        public string ClassCode { get; set; } 
        public List<StudySetItemVM> StudySets { get; set; }
    }
}
