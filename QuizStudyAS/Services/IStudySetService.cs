using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Models;
using QuizStudyAS.ViewModels;

namespace QuizStudyAS.Services
{
    public interface IStudySetService
    {
        Task<List<StudySet>> GetStudySetsByUserIdAsync(string userId);
        Task<StudySet?> GetStudySetByIdAsync(int id);
        Task CreateStudySetAsync(CreateStudySetVM vm, string userId);
        Task<bool> UpdateStudySetAsync(int id, EditStudySetVM vm, string userId);
        Task<object> SearchSuggestionsAsync(string keyword, string userId);
        Task<List<StudySetItemVM>> GetStudySetForClass(string ClassCode);
    }
}