using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Models;
using QuizStudyAS.ViewModels;

namespace QuizStudyAS.Controllers
{
    public class StudySetController : Controller
    {
        private readonly AppDbContext _context;

        public StudySetController(AppDbContext context)
        {
            _context = context;
        }

        // Hàm hỗ trợ lấy ID của user đang đăng nhập từ Session
        private string GetCurrentUserId()
        {
            return HttpContext.Session.GetString("UserId");
        }

        // 1. XEM DANH SÁCH BỘ THẺ CỦA CÁ NHÂN
        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(GetCurrentUserId())) return RedirectToAction("Index", "Home");

            var userId = GetCurrentUserId();
            // Chỉ lấy những bộ thẻ do chính user này tạo
            var mySets = await _context.StudySets
                                       .Where(s => s.OwnerUserId == userId)
                                       .Include(s => s.Flashcards) // Kéo theo số lượng flashcard để hiển thị
                                       .ToListAsync();
            return View(mySets);
        }

        // 2. TẠO MỚI (GET)
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(GetCurrentUserId())) return RedirectToAction("Index", "Home");

            var vm = new CreateStudySetVM();
            // Khởi tạo sẵn 2 thẻ trống để giao diện trông giống Quizlet
            vm.Flashcards.Add(new FlashcardVM());
            vm.Flashcards.Add(new FlashcardVM());

            return View(vm);
        }

        // 3. TẠO MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống tấn công CSRF
        public async Task<IActionResult> Create(CreateStudySetVM vm)
        {
            if (string.IsNullOrEmpty(GetCurrentUserId())) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Lọc bỏ những thẻ bị người dùng để trống cả 2 mặt
            var validFlashcards = vm.Flashcards
                                    .Where(f => !string.IsNullOrWhiteSpace(f.Term) && !string.IsNullOrWhiteSpace(f.Definition))
                                    .ToList();

            if (!validFlashcards.Any())
            {
                ModelState.AddModelError("", "Vui lòng nhập ít nhất một thẻ hợp lệ.");
                return View(vm);
            }

            // Chuyển ViewModel thành Model thực tế để lưu DB
            var studySet = new StudySet
            {
                Title = vm.Title,
                Description = vm.Description,
                OwnerUserId = GetCurrentUserId(), // Gắn cờ quyền sở hữu ngay lập tức
                CreatedAt = DateTime.Now,
                Flashcards = validFlashcards.Select(f => new Flashcard
                {
                    Term = f.Term,
                    Definition = f.Definition
                }).ToList()
            };

            _context.StudySets.Add(studySet);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 4. CHỈNH SỬA (GET) - Đổ dữ liệu cũ lên Form
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            var studySet = await _context.StudySets
                                         .Include(s => s.Flashcards)
                                         .FirstOrDefaultAsync(s => s.StudySetId == id);

            if (studySet == null) return NotFound();

            // RÀNG BUỘC BẢO MẬT: Chỉ chủ sở hữu mới được xem form Edit
            if (studySet.OwnerUserId != userId)
            {
                return RedirectToAction("Index", "Home"); // Hoặc đẩy về trang AccessDenied tùy bạn
            }

            // Chuyển từ Model sang ViewModel để hiển thị lên View
            var vm = new EditStudySetVM
            {
                Id = studySet.StudySetId,
                Title = studySet.Title,
                Description = studySet.Description,
                Flashcards = studySet.Flashcards.Select(f => new FlashcardVM
                {
                    Term = f.Term,
                    Definition = f.Definition
                }).ToList()
            };

            return View(vm);
        }

        // 5. CHỈNH SỬA (POST) - Lưu dữ liệu mới xuống DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditStudySetVM vm)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid) return View(vm);

            var existingSet = await _context.StudySets
                                            .Include(s => s.Flashcards)
                                            .FirstOrDefaultAsync(s => s.StudySetId == id);

            if (existingSet == null) return NotFound();

            // RÀNG BUỘC BẢO MẬT: Chặn Hacker dùng Postman gửi data lên
            if (existingSet.OwnerUserId != userId)
            {
                return RedirectToAction("Index", "Home");
            }

            // Lọc thẻ trống
            var validFlashcards = vm.Flashcards
                                    .Where(f => !string.IsNullOrWhiteSpace(f.Term) && !string.IsNullOrWhiteSpace(f.Definition))
                                    .ToList();

            if (!validFlashcards.Any())
            {
                ModelState.AddModelError("", "Vui lòng nhập ít nhất một thẻ hợp lệ.");
                return View(vm);
            }

            // Cập nhật thông tin cơ bản
            existingSet.Title = vm.Title;
            existingSet.Description = vm.Description;

            // Xóa toàn bộ thẻ cũ trong DB và thay bằng danh sách thẻ mới (Cách an toàn và dễ quản lý index nhất)
            _context.Flashcards.RemoveRange(existingSet.Flashcards);

            existingSet.Flashcards = validFlashcards.Select(f => new Flashcard
            {
                Term = f.Term,
                Definition = f.Definition
            }).ToList();

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // 6. CHỨC NĂNG HỌC THẺ (GET)
        public async Task<IActionResult> Details(int id)
        {
            var studySet = await _context.StudySets
                                         .Include(s => s.Flashcards)
                                         .FirstOrDefaultAsync(s => s.StudySetId == id);

            if (studySet == null) return NotFound();

            // Mở bình luận dòng dưới nếu bạn chỉ muốn 1 mình chủ sở hữu được xem. 
            // Còn nếu muốn chia sẻ link cho bạn bè cùng học thì cứ để trống như thế này.
            // if (studySet.OwnerUserId != GetCurrentUserId()) return RedirectToAction("Index", "Home");

            return View(studySet);
        }
    }

}
