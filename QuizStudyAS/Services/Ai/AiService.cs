using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using QuizStudyAS.DTOs;

namespace QuizStudyAS.Services.Ai
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public AiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Đọc cấu hình từ appsettings.json
            _apiKey = configuration["GeminiSettings:ApiKey"];
            _model = configuration["GeminiSettings:Model"] ?? "gemini-1.5-flash";
        }

        public async Task<ServiceResult<string>> GetAnswerAsync(string userMessage)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

                // Đóng gói dữ liệu đầu vào theo chuẩn JSON của Google Gemini
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[] { new { text = userMessage } }
                        }
                    }
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                // Bắn Request sang server của Google
                var response = await _httpClient.PostAsync(url, jsonContent);

                // Nếu Google báo lỗi (VD: sai Key, quá giới hạn)
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(errorContent);
                    return ServiceResult<string>.IsError($"Lỗi từ máy chủ AI ({response.StatusCode}). Vui lòng thử lại sau.");
                }

                var responseString = await response.Content.ReadAsStringAsync();

                // Bóc tách JSON để lấy đúng câu trả lời dạng Text
                using var doc = JsonDocument.Parse(responseString);
                var textResponse = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                Console.WriteLine(textResponse);
                return ServiceResult<string>.IsSuccess(textResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ServiceResult<string>.IsError($"Không thể kết nối đến AI. Chi tiết: {ex.Message}");
            }
        }
    }
}