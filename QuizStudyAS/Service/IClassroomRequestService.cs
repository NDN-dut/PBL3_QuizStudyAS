using QuizStudyAS.Models;

namespace QuizStudyAS.Service
{
    public interface IClassroomRequestService
    {
        public  Task CreateRequest(string ClassName);
    }
}
