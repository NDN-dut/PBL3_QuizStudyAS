namespace QuizStudyAS.Models
{
    public class ClassRoomMaterial
    {
        public int ClassRoomId {  get; set; }
        public int StudySetId {  get; set; }
        public string Status {  get; set; }

        public virtual Classroom ClassRoom { get; set; }
        public virtual StudySet StudySet { get; set; }

    }
}
