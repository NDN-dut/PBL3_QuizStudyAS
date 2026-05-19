using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Services;
using QuizStudyAS.ViewModels;

namespace QuizStudyAS.Controllers
{
    public class StudySetController : Controller
    {
        private readonly IStudySetService _studySetService;

        // Controller CHỈ tiêm Service, tuyệt đối không biết gì về AppDbContext
        public StudySetController(IStudySetService studySetService)
        {
            _studySetService = studySetService;
        }

        private string? GetCurrentUserId()
        {
            return HttpContext.Session.GetString("UserId");
        }

        // 1. XEM DANH SÁCH BỘ THẺ CỦA CÁ NHÂN
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            var mySets = await _studySetService.GetStudySetsByUserIdAsync(userId);
            return View(mySets);
        }

        // 2. TẠO MỚI (GET)
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(GetCurrentUserId())) return RedirectToAction("Index", "Home");

            var vm = new CreateStudySetVM();
            vm.Flashcards.Add(new FlashcardVM());
            vm.Flashcards.Add(new FlashcardVM());

            return View(vm);
        }
        // 3. TẠO MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudySetVM vm)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid) return View(vm);

            var validFlashcards = vm.Flashcards.Where(f => !string.IsNullOrWhiteSpace(f.Term) && !string.IsNullOrWhiteSpace(f.Definition));
            if (!validFlashcards.Any())
            {
                ModelState.AddModelError("", "Vui lòng nhập ít nhất một thẻ hợp lệ.");
                return View(vm);
            }

            // Giao việc cho Service xử lý
            await _studySetService.CreateStudySetAsync(vm, userId);

            return RedirectToAction(nameof(Index));
        }

        // 4. CHỈNH SỬA (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            var studySet = await _studySetService.GetStudySetByIdAsync(id);

            if (studySet == null) return NotFound();
            if (studySet.OwnerUserId != userId) return RedirectToAction("AccessDenied", "Auth");

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

        // 5. CHỈNH SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditStudySetVM vm)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var validFlashcards = vm.Flashcards.Where(f => !string.IsNullOrWhiteSpace(f.Term) && !string.IsNullOrWhiteSpace(f.Definition));
            if (!validFlashcards.Any())
            {
                ModelState.AddModelError("", "Vui lòng nhập ít nhất một thẻ hợp lệ.");
                return View(vm);
            }

            // Giao việc cho Service xử lý cập nhật
            var success = await _studySetService.UpdateStudySetAsync(id, vm, userId);

            if (!success) return RedirectToAction("AccessDenied", "Auth");

            return RedirectToAction(nameof(Index));
        }

        // 6. CHỨC NĂNG HỌC THẺ (GET)
        public async Task<IActionResult> Details(int id)
        {
            var studySet = await _studySetService.GetStudySetByIdAsync(id);
            if (studySet == null) return NotFound();

            return View(studySet);
        }

        // 7. API TÌM KIẾM MỜ (AUTO-SUGGESTION)
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string keyword)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(keyword))
            {
                return Json(new List<object>());
            }

            var suggestions = await _studySetService.SearchSuggestionsAsync(keyword, userId);
            return Json(suggestions);
        }
    }
}