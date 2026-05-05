using QuizStudyAS.ViewModels;

namespace QuizStudyAS.Services
{
    public interface IClassRoomServices
    {
        public Task<ShowClassRoom> FindClassRoomByName(string NameClass);
        public Task CreateRequest(string ClassName);

        public Task<YourClassVM> GetYourClass();
    }
}
