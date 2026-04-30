using QuizStudyAS.Models;

namespace QuizStudyAS.Services
{
    public interface IClassroomRequestService
    {
        public Task CreateRequest(string ClassName);
    }
}
