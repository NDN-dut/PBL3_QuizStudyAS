

using QuizStudyAS.DTO;

namespace QuizStudyAS.Service
{
    public interface IFindClassRoomService
    {
        public Task<ShowClassRoom> FindClassRoomByName(string NameClass); 
    }
}
