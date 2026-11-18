using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.ViewModels;

namespace QuizStudyAS.Services.ClassRoom
{
    public interface IClassRoomServices
    {
        public Task<ListShowClassRoomVM> FindClassRoomByName(string NameClass);
        public Task CreateRequest(string classCode);
        public Task<YourClassVM> GetYourClass();
        public Task<ListRequestJoinVM> GetJoinVMs();
        public Task<bool> DeniedRequest(string userid, int classroomid);
        public Task<bool> AcceptRequest(string userid, int classroomid);
        public Task CreateClassRoom(string ClassName);
        public Task<string> CreateUniqueLink();
        public Task<ClassRoomDetailVM> GetClassRoomDetail(string LinkLop);
        public Task<bool> AddStudySet(string ClassCode, int StudySetId);
        public Task<bool> DeleteStudySet(string ClassCode, int StudySetId);
        public Task<bool> CheckAuthorityClass(int ClassId);
        public Task<bool> IsMemberOrOwner(string classCode);
        public Task<bool> DeleteUserOfClass(string ClassCode,string UserId);
        public Task<bool> DeleteClassRoom(string Classcode);
        public Task<bool> LeaveClassRoom(string classCode);
        public Task<string?> RegenerateInviteCode(string oldClassCode);
        public Task<ClassRoomDetailVM> GetAllUserOfClass(string ClassCode);
        public Task<bool> CreatePost(string classCode, string content);
        public Task<bool> DeletePost(int postId);
    }
}
