using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Models;
using QuizStudyAS.ViewModels;

namespace QuizStudyAS.Services
{
    // ĐÃ ĐƯA RA NGOÀI: Class DTO khai báo độc lập để mọi nơi đều dùng được
    public class ClassroomShareDTO
    {
        public int ClassroomId { get; set; }
        public string ClassName { get; set; }
        public string ClassCode {  get; set; }
        public bool IsAdded { get; set; } // Kiểm tra xem bộ thẻ đã nằm trong lớp này chưa
    }

    public interface IStudySetService
    {
        Task<List<StudySet>> GetStudySetsByUserIdAsync(string userId);
        Task<StudySet?> GetStudySetByIdAsync(int id);
        Task CreateStudySetAsync(CreateStudySetVM vm, string userId);
        Task<bool> UpdateStudySetAsync(int id, EditStudySetVM vm, string userId);
        Task<object> SearchSuggestionsAsync(string keyword, string userId);
        Task<List<StudySetItemVM>> GetStudySetForClass(string ClassCode);

        // Nhận diện ClassroomShareDTO bình thường
        Task<List<ClassroomShareDTO>> GetClassesForSharingAsync(string userId, int studySetId);
        Task<bool> AddStudySetToClassAsync(int studySetId, int classroomId);

        Task<StudySetInventoryVM> GetInventoryByUserIdAsync(string userId);
        Task<bool> DeleteStudySetAsync(int id, string userId);
    }
}