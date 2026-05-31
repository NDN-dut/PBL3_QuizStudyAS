using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.ViewModels;

namespace QuizStudyAS.Services
{
    public interface IClassRoomServices
    {
        public Task<ListShowClassRoomVM> FindClassRoomByName(string NameClass);
        public Task CreateRequest(string ClassName);
        public Task<YourClassVM> GetYourClass();
        public Task<ListRequestJoinVM> GetJoinVMs();
        public Task DeninedRequest(string userid, int classroomid);
        public Task AcceptRequest(string userid, int classroomid);
        public Task CreateClassRoom(string ClassName);
        public Task<string> CreateUniqueLink();
        public Task<ClassRoomDetailVM> GetClassRoomDetail(string LinkLop);
        public Task<bool> AddStudySet(string ClassCode, int StudySetId);
        public Task<bool> DeleteStudySet(string ClassCode, int StudySetId);
        public Task<bool> CheckAuthorityClass(int ClassId);
        public Task<ClassRoomDetailVM> GetAllUserOfClass(int ClassId);
    }
}
