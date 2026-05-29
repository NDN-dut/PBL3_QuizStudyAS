using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Models;
using QuizStudyAS.ViewModels;
using Microsoft.AspNetCore.Http; // Thư viện cần cho IHttpContextAccessor của bạn
using static QuizStudyAS.Services.IStudySetService; // Thư viện để nhận diện DTO mới

namespace QuizStudyAS.Services
{
    public class StudySetService : IStudySetService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // GIỮ NGUYÊN Constructor cũ của bạn
        public StudySetService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
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
                .Include(s => s.OwnerUser)
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
                Description = vm.Description ?? "",
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

            if (existingSet == null || existingSet.OwnerUserId != userId)
            {
                return false;
            }

            var validFlashcards = vm.Flashcards
                .Where(f => !string.IsNullOrWhiteSpace(f.Term) && !string.IsNullOrWhiteSpace(f.Definition))
                .ToList();

            existingSet.Title = vm.Title;
            existingSet.Description = vm.Description ?? "";
            existingSet.UpdatedAt = DateTime.Now;

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
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<object>();
            }

            var searchTerms = keyword.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // 1. Lấy tất cả ID lớp học liên quan đến người dùng (Sở hữu + Tham gia)
            var ownedClassIds = await _context.Classrooms
                .Where(c => c.OwnerUserId == userId)
                .Select(c => c.ClassroomId)
                .ToListAsync();

            var joinedClassIds = await _context.ClassroomUsers
                .Where(cu => cu.UserId == userId)
                .Select(cu => cu.ClassroomId)
                .ToListAsync();

            var allMyClassIds = ownedClassIds.Union(joinedClassIds).Distinct().ToList();

            // 2. Thu thập ID của tất cả học phần được chia sẻ công khai trong các lớp này
            var sharedStudySetIds = new List<int>();
            if (allMyClassIds.Any())
            {
                sharedStudySetIds = await _context.ClassRoomMaterials
                    .Where(cm => allMyClassIds.Contains(cm.ClassRoomId) && cm.Status == "AVAILABLE")
                    .Select(cm => cm.StudySetId)
                    .ToListAsync();
            }

            // 3. Tạo câu truy vấn hợp nhất: Tìm trong học phần TỰ TẠO hoặc học phần THUỘC LỚP HỌC
            var query = _context.StudySets
                .Where(s => s.OwnerUserId == userId || sharedStudySetIds.Contains(s.StudySetId));

            // 4. Tiến hành lọc mờ (Fuzzy Filter) theo từng từ khóa
            foreach (var term in searchTerms)
            {
                query = query.Where(s => s.Title.ToLower().Contains(term));
            }

            // 5. Trả về cấu trúc dữ liệu nén gọn nhẹ (Giữ nguyên cấu trúc để Frontend không bị lỗi)
            return await query
                .Select(s => new {
                    id = s.StudySetId,
                    title = s.Title,
                    cardCount = s.Flashcards.Count
                })
                .Take(5)
                .ToListAsync();
        }

        // GIỮ NGUYÊN hàm cũ của bạn với logic check MaterialsOf
        public async Task<List<StudySetItemVM>> GetStudySetForClass(string ClassCode)
        {
            var ClassId = await _context.Classrooms.Where(c => c.InviteCode == ClassCode).Select(c => c.ClassroomId).FirstOrDefaultAsync();
            string CurrentUserId = _httpContextAccessor.HttpContext.Session.GetString("UserId");
            var MyStudySet = await _context.StudySets.Where(s => s.OwnerUserId == CurrentUserId)
                                                            .Select(s => new StudySetItemVM
                                                            {
                                                                StudySetId = s.StudySetId,
                                                                Title = s.Title,
                                                                Status = s.MaterialsOf.Where(e => e.ClassRoomId == ClassId).Select(e => e.Status).FirstOrDefault() ?? "NOT_ADD"
                                                            })
                                                            .ToListAsync();
            return MyStudySet;
        }

        // ========================================================
        // 2 HÀM MỚI BỔ SUNG CHO TÍNH NĂNG "THÊM VÀO LỚP HỌC"
        // ========================================================

        public async Task<List<ClassroomShareDTO>> GetClassesForSharingAsync(string userId, int studySetId)
        {
            // 1. CHỈ LẤY DANH SÁCH CÁC LỚP DO CHÍNH USER NÀY TẠO RA (LÀM CHỦ)
            var ownedClasses = await _context.Classrooms
                .Where(c => c.OwnerUserId == userId)
                .ToListAsync();

            // 2. Lấy ID các lớp đã được thêm bộ thẻ này (kiểm tra trạng thái Đã thêm)
            var sharedClassIds = await _context.ClassRoomMaterials
                .Where(cm => cm.StudySetId == studySetId)
                .Select(cm => cm.ClassRoomId)
                .ToListAsync();

            // 3. Trả về kết quả
            return ownedClasses.Select(c => new ClassroomShareDTO
            {
                ClassroomId = c.ClassroomId,
                ClassName = c.ClassName,
                IsAdded = sharedClassIds.Contains(c.ClassroomId)
            }).ToList();
        }

        public async Task<bool> AddStudySetToClassAsync(int studySetId, int classroomId)
        {
            // Bảng ClassRoomMaterials dùng ClassRoomId viết hoa chữ R
            var exists = await _context.ClassRoomMaterials
                .AnyAsync(cm => cm.StudySetId == studySetId && cm.ClassRoomId == classroomId);

            if (exists) return true;

            var material = new ClassRoomMaterial
            {
                ClassRoomId = classroomId,
                StudySetId = studySetId,
                Status = "Active"
            };

            _context.ClassRoomMaterials.Add(material);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<StudySetInventoryVM> GetInventoryByUserIdAsync(string userId)
        {
            var inventory = new StudySetInventoryVM();

            // 1. Lấy Kho Bí Kíp Cá Nhân (Tự tạo)
            inventory.PersonalGroup.GroupName = "Kho Bí Kíp Của Tôi";
            inventory.PersonalGroup.StudySets = await _context.StudySets
                .Where(s => s.OwnerUserId == userId)
                .Include(s => s.Flashcards)
                .Include(s => s.OwnerUser)
                .ToListAsync();

            // 2. Thu thập tất cả ID Lớp học (Do mình tạo + Tham gia)
            var ownedClassIds = await _context.Classrooms
                .Where(c => c.OwnerUserId == userId)
                .Select(c => c.ClassroomId)
                .ToListAsync();

            var joinedClassIds = await _context.ClassroomUsers
                .Where(cu => cu.UserId == userId)
                .Select(cu => cu.ClassroomId)
                .ToListAsync();

            var allMyClassIds = ownedClassIds.Union(joinedClassIds).Distinct().ToList();

            // 3. Quét các học phần đang được Share trong các Lớp học này
            if (allMyClassIds.Any())
            {
                var classMaterials = await _context.ClassRoomMaterials
                    .Include(cm => cm.ClassRoom)
                    .Include(cm => cm.StudySet)
                        .ThenInclude(s => s.Flashcards) // Lấy số lượng thẻ
                    .Include(cm => cm.StudySet)
                        .ThenInclude(s => s.OwnerUser) // Lấy tên người tạo thẻ
                    .Where(cm => allMyClassIds.Contains(cm.ClassRoomId) && cm.Status == "AVAILABLE")
                    .ToListAsync();

                // Gom nhóm (Group By) theo từng lớp học
                var grouped = classMaterials.GroupBy(cm => cm.ClassRoom);
                foreach (var group in grouped)
                {
                    inventory.ClassGroups.Add(new StudySetGroupVM
                    {
                        ClassroomId = group.Key.ClassroomId,
                        GroupName = $"Học phần lớp: {group.Key.ClassName}",
                        StudySets = group.Select(cm => cm.StudySet).ToList()
                    });
                }
            }

            return inventory;
        }
    }
}