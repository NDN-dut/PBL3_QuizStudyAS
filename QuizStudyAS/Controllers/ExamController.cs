using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.DTOs;
using QuizStudyAS.Services;
using QuizStudyAS.ViewModels;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QuizStudyAS.Controllers
{
    public class ExamController : Controller
    {
        private readonly IExamService _examService;

        public ExamController(IExamService examService)
        {
            _examService = examService;
        }

        private string? GetCurrentUserId()
        {
            return HttpContext.Session.GetString("UserId");
        }

        [HttpGet]
        public async Task<IActionResult> MyExams()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var result = await _examService.GetMyExamsAsync();

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            return View(result.Data); // Truyền MyExamsVM xuống View
        }

        // 1. API BẮT ĐẦU LÀM BÀI (Hoặc tiếp tục bài đang làm dở)
        [HttpGet]
        public async Task<IActionResult> TakeExam(int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Gọi xuống tầng Service để xử lý logic thời gian và tạo lượt làm bài
            var result = await _examService.StartExamAsync(id);

            if (!result.Success)
            {
                // Nếu có lỗi (chưa đến giờ, đã nộp rồi,...), thông báo và đẩy về trang trước đó
                TempData["ErrorMessage"] = result.Message;
                string referer = Request.Headers["Referer"].ToString();
                if (string.IsNullOrEmpty(referer))
                    return RedirectToAction("Index", "Home");

                return Redirect(referer);
            }

            // Truyền đối tượng ExamAttempt xuống View để hiển thị câu hỏi và đồng hồ đếm ngược
            return View(result.Data);
        }

        // 2. API NỘP BÀI (Gọi qua AJAX/Fetch API từ giao diện người dùng)
        [HttpPost]
        public async Task<IActionResult> SubmitExam([FromBody] SubmitExamRequestDTO request)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
            }

            if (request == null)
            {
                return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ." });
            }

            if (request.Answers == null)
            {
                request.Answers = new List<StudentAnswerDTO>();
            }

            // Gọi xuống Service bằng các thuộc tính của DTO
            var result = await _examService.SubmitExamAsync(request.AttemptId, request.Answers);

            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new
            {
                success = true,
                message = result.Message,
                score = result.Data
            });
        }
        // 3. API HIỂN THỊ FORM TẠO BÀI KIỂM TRA
        [HttpGet]
        public IActionResult Create(int classroomId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Khởi tạo các giá trị mặc định cho form
            var vm = new CreateExamVM
            {
                ClassroomId = classroomId,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddDays(1),
                DurationMinutes = 45 // Mặc định 45 phút
            };

            return View(vm);
        }

        // 4. API XỬ LÝ LƯU BÀI KIỂM TRA & ĐỌC FILE CSV
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateExamVM model)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Gọi xuống Service để xử lý file CSV và lưu vào Database
            var result = await _examService.CreateExamFromCsvAsync(model, userId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                // Tạm thời quay về danh sách Lớp học. 
                // Sau này bạn có thể đổi thành quay về trang Chi tiết Lớp học (ClassRoomDetail)
                return RedirectToAction("Index", "Classroom");
            }
            else
            {
                // Nếu lỗi (ví dụ: file sai format, thời gian sai), báo lỗi ra màn hình
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }
        }
        // 5. API HIỂN THỊ GIAO DIỆN XEM ĐIỂM
        [HttpGet]
        public async Task<IActionResult> Results(int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var result = await _examService.GetExamResultsAsync(id, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index", "Classroom");
            }

            return View(result.Data);
        }

        // 6. API XUẤT FILE CSV KẾT QUẢ BÀI THI
        [HttpGet]
        public async Task<IActionResult> ExportResultsCsv(int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _examService.GetExamResultsAsync(id, userId);
            if (!result.Success) return BadRequest(result.Message);

            var vm = result.Data;
            var builder = new StringBuilder();

            // Thêm ký tự BOM (Byte Order Mark) để Excel đọc đúng font Tiếng Việt UTF-8
            builder.Append('\uFEFF');

            // Ghi dòng tiêu đề
            builder.AppendLine("Mã tài khoản,Tên học sinh,Trạng thái,Điểm số,Thời gian bắt đầu,Thời gian nộp");

            // Ghi từng dòng dữ liệu
            foreach (var st in vm.StudentResults)
            {
                var scoreStr = st.Score.HasValue ? st.Score.Value.ToString() : "";
                var startStr = st.StartedAt.HasValue ? st.StartedAt.Value.ToString("HH:mm dd/MM/yyyy") : "";
                var endStr = st.CompletedAt.HasValue ? st.CompletedAt.Value.ToString("HH:mm dd/MM/yyyy") : "";

                // Bọc các cột rủi ro (như Tên) vào ngoặc kép để tránh lỗi vỡ cột nếu tên có chứa dấu phẩy
                builder.AppendLine($"\"{st.UserId}\",\"{st.UserName}\",\"{st.Status}\",\"{scoreStr}\",\"{startStr}\",\"{endStr}\"");
            }

            var fileBytes = Encoding.UTF8.GetBytes(builder.ToString());
            return File(fileBytes, "text/csv", $"KetQua_{vm.ExamId}.csv");
        }
        [HttpGet]
        public async Task<IActionResult> Review(int attemptId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Auth");

            var result = await _examService.GetExamReviewAsync(attemptId, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                // Nếu học sinh cố tình gõ link này, sẽ bị đá văng về trang chủ
                return RedirectToAction("Index", "Home");
            }

            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetPendingExamCount()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { count = 0 });
            }

            int count = await _examService.GetPendingExamsCountAsync(userId);
            return Json(new { count = count });
        }
    }
}