
using QuizStudyAS.DTO;

namespace QuizStudyAS.Services
{
    public interface IFindClassRoomService
    {
        public Task<ShowClassRoom> FindClassRoomByName(string NameClass);
    }
}

