using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuizStudyAS.Controllers
{
    public class ChatController : Controller
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public ChatController(IConfiguration configuration)
        {
            // Lấy API Key an toàn từ appsettings.json
            _apiKey = configuration["Gemini:ApiKey"];
            _httpClient = new HttpClient();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestInput input)
        {
            if (string.IsNullOrWhiteSpace(input?.Message))
            {
                return Json(new { success = false, reply = "Tin nhắn không được để trống cậu ơi!" });
            }

            try
            {
                // 1. Định nghĩa URL gọi tới mô hình Gemini 1.5 Flash (Nhanh, thông minh, tiết kiệm)
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

                // 2. Định nghĩa System Instruction (Tính cách, nhiệm vụ ép buộc cho Bé Ma)
                string systemPrompt = "Bạn là 'Bé Ma AI' 👻, trợ lý ảo siêu cấp đáng yêu và thông minh của website học tập trực tuyến QuizStudyAS. " +
                                      "Nhiệm vụ của bạn là hỗ trợ học viên giải đáp kiến thức và đặc biệt là TẠO ĐỊNH DẠNG FLASHCARD. " +
                                      "Nếu người dùng yêu cầu tạo flashcard/từ vựng, hãy luôn trình bày câu trả lời rõ ràng dưới dạng danh sách gồm: Thuật ngữ - Định nghĩa để họ dễ nhìn. " +
                                      "Hãy nói chuyện thân thiện, thỉnh thoảng dùng icon dễ thương thích hợp với vibe học tập.";

                // 3. Tạo cấu trúc Payload JSON theo đúng tài liệu kỹ thuật của Google
                var payload = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = input.Message } } }
                    },
                    systemInstruction = new
                    {
                        parts = new[] { new { text = systemPrompt } }
                    }
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // 4. Bắn Request sang Google Server
                HttpResponseMessage response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { success = false, reply = "Bé Ma đang mất kết nối với vũ trụ AI rồi, thử lại sau nhé!" });
                }

                string responseString = await response.Content.ReadAsStringAsync();

                // 5. Bóc tách dữ liệu JSON trả về từ Google để lấy chuỗi văn bản câu trả lời
                using var jsonDoc = JsonDocument.Parse(responseString);
                string aiReply = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return Json(new { success = true, reply = aiReply });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, reply = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }

    // Class hứng dữ liệu chữ từ Frontend đẩy lên
    public class ChatRequestInput
    {
        public string Message { get; set; }
    }
}
