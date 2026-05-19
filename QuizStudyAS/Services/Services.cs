using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Models;
using QuizStudyAS.ViewModels;

namespace QuizStudyAS.Services
{
    public class StudySetService : IStudySetService
    {
        private readonly AppDbContext _context;

        // Tầng Service MỚI LÀ NƠI được phép tiêm AppDbContext
        public StudySetService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudySet>> GetStudySetsByUserIdAsync(string userId)
        {
            return await _context.StudySets
                .Where(s => s.OwnerUserId == userId)
                .Include(s => s.Flashcards)
                .ToListAsync();
        }

        public async Task<StudySet?> GetStudySetByIdAsync(int id)
        {
            return await _context.StudySets
                .Include(s => s.Flashcards)
                .FirstOrDefaultAsync(s => s.StudySetId == id);
        }

        public async Task CreateStudySetAsync(CreateStudySetVM vm, string userId)
        {
            var validFlashcards = vm.Flashcards
                .Where(f => !string.IsNullOrWhiteSpace(f.Term) && !string.IsNullOrWhiteSpace(f.Definition))
                .ToList();

            var studySet = new StudySet
            {
                Title = vm.Title,
                Description = vm.Description ?? "", // Xử lý lỗi null Description tại đây
                OwnerUserId = userId,
                CreatedAt = DateTime.Now,
                Flashcards = validFlashcards.Select(f => new Flashcard
                {
                    Term = f.Term,
                    Definition = f.Definition
                }).ToList()
            };

            _context.StudySets.Add(studySet);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateStudySetAsync(int id, EditStudySetVM vm, string userId)
        {
            var existingSet = await _context.StudySets
                .Include(s => s.Flashcards)
                .FirstOrDefaultAsync(s => s.StudySetId == id);

            // Kiểm tra tồn tại và quyền sở hữu
            if (existingSet == null || existingSet.OwnerUserId != userId)
            {
                return false;
            }

            var validFlashcards = vm.Flashcards
                .Where(f => !string.IsNullOrWhiteSpace(f.Term) && !string.IsNullOrWhiteSpace(f.Definition))
                .ToList();

            existingSet.Title = vm.Title;
            existingSet.Description = vm.Description ?? "";

            // Xóa thẻ cũ, thay thẻ mới
            _context.Flashcards.RemoveRange(existingSet.Flashcards);

            existingSet.Flashcards = validFlashcards.Select(f => new Flashcard
            {
                Term = f.Term,
                Definition = f.Definition
            }).ToList();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> SearchSuggestionsAsync(string keyword, string userId)
        {
            var searchTerms = keyword.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var query = _context.StudySets.Where(s => s.OwnerUserId == userId);

            foreach (var term in searchTerms)
            {
                query = query.Where(s => s.Title.ToLower().Contains(term));
            }

            return await query
                .Select(s => new {
                    id = s.StudySetId,
                    title = s.Title,
                    cardCount = s.Flashcards.Count
                })
                .Take(5)
                .ToListAsync();
        }
    }
}