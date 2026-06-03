using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Services;
using QuizStudyAS.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        public async Task<IActionResult> SubmitExam(int attemptId, [FromBody] List<StudentAnswerDTO> answers)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
            }

            // Đề phòng trường hợp Frontend gửi mảng rỗng hoặc null lên
            if (answers == null)
            {
                answers = new List<StudentAnswerDTO>();
            }

            // Service sẽ lo toàn bộ việc chấm điểm và kiểm tra độ trễ mạng
            var result = await _examService.SubmitExamAsync(attemptId, answers);

            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }

            // Trả về số điểm và thông báo (kể cả thông báo nộp trễ nếu có)
            return Json(new
            {
                success = true,
                message = result.Message,
                score = result.Data
            });
        }
    }
}