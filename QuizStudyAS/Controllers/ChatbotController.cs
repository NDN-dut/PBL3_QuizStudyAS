using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Services;
using System.Threading.Tasks;

namespace QuizStudyAS.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly IAiService _aiService;

        public ChatbotController(IAiService aiService)
        {
            _aiService = aiService;
        }

        // 1. Mở trang giao diện Chat
        [HttpGet]
        public IActionResult Index()
        {
            // Tùy chọn: Chặn không cho khách chưa đăng nhập dùng AI
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["LockedMessage"] = "Vui lòng đăng nhập để sử dụng Trợ lý AI.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // 2. Nhận tin nhắn từ Javascript đẩy lên và trả về JSON
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return Json(new { success = false, message = "Tin nhắn không được để trống." });
            }

            // Gọi "Bộ não" AI
            var result = await _aiService.GetAnswerAsync(request.Message);

            // Trả kết quả về cho Frontend dưới dạng JSON
            return Json(new
            {
                success = result.Success,
                data = result.Data,
                message = result.Message
            });
        }
    }

    // Class phụ trợ để hứng dữ liệu JSON từ Frontend gửi lên
    public class ChatRequest
    {
        public string Message { get; set; }
    }
}