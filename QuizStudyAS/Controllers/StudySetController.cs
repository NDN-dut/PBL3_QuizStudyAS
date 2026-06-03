using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Services;
using QuizStudyAS.ViewModels;
using QuizStudyAS.Models; // THÊM DÒNG NÀY

namespace QuizStudyAS.Controllers
{
    public class StudySetController : Controller
    {
        private readonly IStudySetService _studySetService;
        private readonly IGamificationService _gamificationService; // THÊM DÒNG NÀY
        private readonly IClassRoomServices _classRoomServices; // THÊM DÒNG NÀY

        // Cập nhật Constructor
        public StudySetController(IStudySetService studySetService, IGamificationService gamificationService, IClassRoomServices classRoomServices)
        {
            _studySetService = studySetService;
            _gamificationService = gamificationService; // THÊM DÒNG NÀY
            _classRoomServices = classRoomServices; // THÊM DÒNG NÀY
        }

        // ... (Các hàm GetCurrentUserId() và Index() giữ nguyên) ...

        private string? GetCurrentUserId()
        {
            return HttpContext.Session.GetString("UserId");
        }

        // 1. XEM DANH SÁCH BỘ THẺ THEO NHÓM
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            // Đổi từ GetStudySetsByUserIdAsync sang gọi hàm Inventory mới
            var inventory = await _studySetService.GetInventoryByUserIdAsync(userId);

            return View(inventory);
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

        // 6. CHỨC NĂNG LẬT THẺ GHI NHỚ (Được đổi tên từ Details cũ)
        // Khi người dùng bấm vào nút "Thẻ ghi nhớ" từ trang Hub, hệ thống sẽ chạy hàm này
        public async Task<IActionResult> Learn(int id)
        {
            var studySet = await _studySetService.GetStudySetByIdAsync(id);
            if (studySet == null) return NotFound();

            // THAY THẾ DÒNG CŨ: if (!studySet.IsActive)
                if (studySet.StatusId != (int)StudySetStatusEnum.Active)
                    return RedirectToRefererWithLockMessage("Học phần này đã bị khóa bởi Quản trị viên hệ thống.");

            // Lệnh này sẽ tự động tìm và render file Views/StudySet/Learn.cshtml 
            // (Nơi chứa giao diện lật thẻ 3D cũ)
            return View(studySet);
        }

        // 6.5. TRANG TRUNG TÂM ĐIỀU HƯỚNG BỘ THẺ (STUDY HUB)
        // Khi người dùng click vào một bộ thẻ từ danh sách Index, hệ thống sẽ chạy hàm này đầu tiên
        public async Task<IActionResult> Details(int id)
        {
            var studySet = await _studySetService.GetStudySetByIdAsync(id);
            if (studySet == null) return NotFound();

            // THAY THẾ DÒNG CŨ: if (!studySet.IsActive)
            if (studySet.StatusId != (int)StudySetStatusEnum.Active)
                return RedirectToRefererWithLockMessage("Học phần này đã bị khóa bởi Quản trị viên hệ thống.");

            // Lệnh này sẽ tự động tìm và render file Views/StudySet/Details.cshtml
            // (Nơi chứa giao diện Hub mới gồm 3 nút chức năng)
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

        // --- API CHO TÍNH NĂNG "THÊM VÀO LỚP HỌC" ---

        // ĐỔI TÊN BIẾN classroomId THÀNH classCode ĐỂ KHỚP VỚI HÀM CỦA ClassRoomServices
        public class AddToClassRequest { public int studySetId { get; set; } public string classCode { get; set; } }

        [HttpGet]
        public async Task<IActionResult> GetMyClassesForShare(int studySetId)
        {
            // 1. Lấy ID người dùng đang đăng nhập (Tùy theo cách nhóm bạn lưu Session)
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new List<object>()); // Trả về mảng rỗng nếu chưa đăng nhập
            }

            // 2. GỌI ĐÚNG HÀM Ở TẦNG SERVICE (Hàm này đã được lọc chỉ lấy lớp tự tạo)
            var classes = await _studySetService.GetClassesForSharingAsync(userId, studySetId);

            return Json(classes);
        }
        [HttpPost]
        public async Task<IActionResult> AddSetToClass([FromBody] AddToClassRequest req)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Json(new { success = false, message = "Vui lòng đăng nhập" });
            
            // GỌI THẲNG XUỐNG SERVICE: Truyền đúng Mã code lớp và ID bộ thẻ
            bool isSuccess = await _classRoomServices.AddStudySet(req.classCode, req.studySetId);
            
            if (isSuccess)
            {
                return Json(new { success = true, message = "Đã chia sẻ học phần vào lớp thành công!" });
            }

            return Json(new { success = false, message = "Bạn không có quyền hoặc lớp học không tồn tại." });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyStudySetsForClass(string ClassCode)
        {
            List<StudySetItemVM> result = await _studySetService.GetStudySetForClass(ClassCode);
            return Json(result);
        }

        // --- CHẾ ĐỘ GAME ĐA NĂNG (TRẮC NGHIỆM / TỰ LUẬN / TRỘN LẪN) ---
        [HttpGet]
        public async Task<IActionResult> Quiz(int id, string mode = "multiple", int timer = 15)
        {
            var studySet = await _studySetService.GetStudySetByIdAsync(id);
            if (studySet == null) return NotFound();

            // THAY THẾ DÒNG CŨ: if (!studySet.IsActive)
            if (studySet.StatusId != (int)StudySetStatusEnum.Active)
                return RedirectToRefererWithLockMessage("Học phần này đã bị khóa bởi Quản trị viên hệ thống.");

            // 1. Ràng buộc cơ bản: Phải có ít nhất 1 thẻ để chơi (dành cho chế độ Tự luận)
            if (studySet.Flashcards.Count < 1)
            {
                TempData["ErrorMessage"] = "Học phần này chưa có thuật ngữ nào để chơi!";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // 2. Ràng buộc trắc nghiệm: Nếu chọn Trắc nghiệm hoặc Trộn lẫn, phải có >= 4 thẻ
            if ((mode == "multiple" || mode == "mixed") && studySet.Flashcards.Count < 4)
            {
                // Gắn câu thông báo bạn yêu cầu vào TempData
                TempData["ErrorMessage"] = "Học phần này không đủ số thẻ quy định. Cần ít nhất 4 thẻ để tạo bộ câu hỏi trắc nghiệm!!";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // Gửi cấu hình lựa chọn xuống View thông qua ViewBag
            ViewBag.GameMode = mode;
            ViewBag.GameTimer = timer;

            return View(studySet);
        }
        // ==========================================
        // API: LƯU TIẾN ĐỘ GAME HÓA (XP, STREAK)
        // Gọi bằng AJAX từ View khi người dùng hoàn thành bài
        // ==========================================
        // ==========================================
        // API: LƯU TIẾN ĐỘ GAME HÓA (XP, STREAK) VÀ ĐỒNG BỘ SESSION
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> SaveProgress([FromBody] ProgressRequest req)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Json(new { success = false, message = "Vui lòng đăng nhập" });

            // Tính điểm XP thưởng: Quiz được 50 XP, Học lật thẻ được 10 XP
            int earnedXP = req.ActionType == "Quiz" ? 50 : 10;

            // Gọi Service xử lý lưu vào CSDL
            var result = await _gamificationService.UpdateUserProgressAsync(userId, earnedXP);

            if (result.Success)
            {
                // CRITICAL: Cập nhật lại Session ngay lập tức để các trang khác tải lên đều nhận số mới
                HttpContext.Session.SetInt32("UserLevel", result.Level);
                HttpContext.Session.SetInt32("UserStreak", result.CurrentStreak);

                return Json(new
                {
                    success = true,
                    level = result.Level,
                    currentStreak = result.CurrentStreak,
                    earnedXP = result.EarnedXP,
                    isLeveledUp = result.IsLeveledUp,
                    isStreakSaved = result.IsStreakSaved
                });
            }

            return Json(new { success = false });
        }

        public class ProgressRequest
        {
            public int StudySetId { get; set; }
            public string ActionType { get; set; } // "Learn" hoặc "Quiz"
            public int Score { get; set; }
        }

        private IActionResult RedirectToRefererWithLockMessage(string message)
        {
            TempData["LockedMessage"] = message;
            string referer = Request.Headers["Referer"].ToString();

            // Nếu không xác định được trang trước đó, đẩy về Trang chủ
            if (string.IsNullOrEmpty(referer))
            {
                return RedirectToAction("Index", "Home");
            }
            return Redirect(referer);
        }
        // 8. XÓA MỀM BỘ THẺ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            // Gọi hàm xóa mềm dưới tầng Service
            var success = await _studySetService.DeleteStudySetAsync(id, userId);

            if (!success)
            {
                // Có thể là do không tìm thấy bộ thẻ, hoặc người dùng không phải là chủ sở hữu
                TempData["ErrorMessage"] = "Có vẻ như không thể xóa học phần này hoặc bạn không có quyền thực hiện.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Học phần đã được xóa thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}