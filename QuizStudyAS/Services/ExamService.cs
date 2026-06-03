using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.DTOs;
using QuizStudyAS.Models;

namespace QuizStudyAS.Services
{
    public class ExamService : IExamService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExamService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResult<ExamAttempt>> StartExamAsync(int examId)
        {
            string currentUserId = _httpContextAccessor.HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return ServiceResult<ExamAttempt>.IsError("Bạn chưa đăng nhập.");

            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null)
                return ServiceResult<ExamAttempt>.IsError("Không tìm thấy bài kiểm tra.");

            var now = DateTime.Now;

            // Kiểm tra xem kì thi đã mở hay đã đóng chưa
            if (now < exam.StartTime)
                return ServiceResult<ExamAttempt>.IsError("Bài kiểm tra chưa đến giờ mở.");
            if (now > exam.EndTime)
                return ServiceResult<ExamAttempt>.IsError("Bài kiểm tra đã đóng.");

            // Kiểm tra xem người dùng đã từng bắt đầu bài này chưa (tránh F5 tạo lượt mới)
            var existingAttempt = await _context.ExamAttempts
                .FirstOrDefaultAsync(a => a.ExamId == examId && a.UserId == currentUserId);

            if (existingAttempt != null)
            {
                if (existingAttempt.IsSubmitted)
                    return ServiceResult<ExamAttempt>.IsError("Bạn đã nộp bài này rồi.");

                // Nếu đang làm dở thì trả về luôn lượt cũ
                return ServiceResult<ExamAttempt>.IsSuccess(existingAttempt, "Tiếp tục làm bài.");
            }

            // Tạo lượt làm bài mới
            var newAttempt = new ExamAttempt
            {
                ExamId = examId,
                UserId = currentUserId,
                StartedAt = now,
                Score = 0,
                IsSubmitted = false
            };

            _context.ExamAttempts.Add(newAttempt);
            await _context.SaveChangesAsync();

            return ServiceResult<ExamAttempt>.IsSuccess(newAttempt, "Bắt đầu làm bài thành công.");
        }

        public async Task<ServiceResult<double>> SubmitExamAsync(int attemptId, List<StudentAnswerDTO> studentAnswers)
        {
            string currentUserId = _httpContextAccessor.HttpContext.Session.GetString("UserId");

            // Lấy lượt làm bài kèm theo Cấu trúc đề thi và Đáp án
            var attempt = await _context.ExamAttempts
                .Include(a => a.Exam)
                    .ThenInclude(e => e.ExamQuestions)
                        .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.UserId == currentUserId);

            if (attempt == null)
                return ServiceResult<double>.IsError("Không tìm thấy lượt làm bài.");

            if (attempt.IsSubmitted)
                return ServiceResult<double>.IsError("Bài này đã được nộp từ trước.");

            // ==========================================
            // XỬ LÝ LOGIC THỜI GIAN VÀ ĐỘ TRỄ MẠNG
            // ==========================================
            var submitTime = DateTime.Now;

            var durationDeadline = attempt.StartedAt.AddMinutes(attempt.Exam.DurationMinutes);
            var actualDeadline = durationDeadline < attempt.Exam.EndTime ? durationDeadline : attempt.Exam.EndTime;

            // Mốc 1: Độ trễ an toàn (10 giây)
            var safeDeadline = actualDeadline.AddSeconds(10);

            // Mốc 2: Giới hạn chịu đựng tối đa (3 phút)
            var hardDeadline = actualDeadline.AddMinutes(3);

            if (submitTime > hardDeadline)
            {
                // Nếu nộp quá trễ so với giới hạn chịu đựng -> Hủy kết quả
                attempt.IsSubmitted = true;
                attempt.CompletedAt = submitTime;
                attempt.Score = 0;
                await _context.SaveChangesAsync();
                return ServiceResult<double>.IsError("Hệ thống từ chối nhận bài vì thời gian nộp trễ vượt quá giới hạn cho phép (3 phút).");
            }

            // Đánh giá xem có bị trễ mạng hay không (vượt quá độ trễ an toàn nhưng chưa tới mức bị hủy)
            bool isLateSubmission = submitTime > safeDeadline;
            attempt.IsLate = isLateSubmission;

            // ==========================================
            // LOGIC CHẤM ĐIỂM
            // ==========================================
            double totalScore = 0;
            double scorePerQuestion = attempt.Exam.ExamQuestions.Any()
                                      ? 10.0 / attempt.Exam.ExamQuestions.Count
                                      : 0;

            foreach (var q in attempt.Exam.ExamQuestions)
            {
                // Tìm đáp án học sinh gửi lên cho câu hỏi này
                var studentAns = studentAnswers.FirstOrDefault(sa => sa.QuestionId == q.QuestionId);

                // Lưu chi tiết từng câu
                var attemptDetail = new ExamAttemptDetail
                {
                    AttemptId = attemptId,
                    QuestionId = q.QuestionId,
                    SelectedOptionId = studentAns?.SelectedOptionId
                };
                _context.ExamAttemptDetails.Add(attemptDetail);

                // Chấm điểm: Nếu có chọn đáp án và đáp án đó đánh dấu IsCorrect = true
                if (studentAns != null && studentAns.SelectedOptionId.HasValue)
                {
                    var isCorrect = q.Options.FirstOrDefault(o => o.OptionId == studentAns.SelectedOptionId)?.IsCorrect ?? false;
                    if (isCorrect)
                    {
                        totalScore += scorePerQuestion;
                    }
                }
            }

            // Cập nhật tổng kết
            attempt.Score = Math.Round(totalScore, 2); // Làm tròn 2 chữ số thập phân
            attempt.IsSubmitted = true;
            attempt.CompletedAt = submitTime;

            await _context.SaveChangesAsync();

            // ==========================================
            // CẬP NHẬT THÔNG BÁO TRẢ VỀ
            // ==========================================
            string returnMessage = isLateSubmission
                ? "Nộp bài thành công. LƯU Ý: Hệ thống ghi nhận bạn đã nộp trễ do độ trễ mạng hoặc treo máy."
                : "Nộp bài thành công!";

            return ServiceResult<double>.IsSuccess(attempt.Score, returnMessage);
        }
    }
}