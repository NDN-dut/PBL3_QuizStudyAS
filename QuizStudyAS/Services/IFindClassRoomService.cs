
using QuizStudyAS.ViewModels;

namespace QuizStudyAS.Services
{
    public interface IFindClassRoomService
    {
        public Task<ShowClassRoom> FindClassRoomByName(string NameClass);
    }
}

