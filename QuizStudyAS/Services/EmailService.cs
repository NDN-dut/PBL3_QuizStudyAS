using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace QuizStudyAS.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // 1. Đọc thông tin từ appsettings.json
            var emailSettings = _config.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var port = int.Parse(emailSettings["Port"] ?? "587");
            var senderEmail = emailSettings["SenderEmail"];
            var appPassword = emailSettings["AppPassword"];
            var senderName = emailSettings["SenderName"];

            // 2. Khởi tạo SmtpClient với máy chủ của Google
            using var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(senderEmail, appPassword),
                EnableSsl = true // Bắt buộc phải là true đối với Gmail
            };

            // 3. Chuẩn bị nội dung bức thư
            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Bật true để có thể gửi email chứa thẻ HTML (nút bấm, màu sắc)
            };

            mailMessage.To.Add(toEmail);

            // 4. Gửi thư đi (Bất đồng bộ)
            await client.SendMailAsync(mailMessage);
        }
    }
}