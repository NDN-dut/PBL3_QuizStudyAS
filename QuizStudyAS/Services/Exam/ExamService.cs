using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.DTOs;
using QuizStudyAS.Models;
using QuizStudyAS.ViewModels;
using System.Text.RegularExpressions;
using System.IO;

namespace QuizStudyAS.Services.Exam
{
    using Exam = QuizStudyAS.Models.Exam;
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

            // SỬA TẠI ĐÂY: Dùng Include để lấy Exam kèm theo toàn bộ Câu hỏi và Đáp án
            var exam = await _context.Exams
                .Include(e => e.ExamQuestions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.ExamId == examId);

            if (exam == null)
                return ServiceResult<ExamAttempt>.IsError("Không tìm thấy bài kiểm tra.");

            var now = DateTime.Now;

            // Kiểm tra xem kì thi đã mở hay đã đóng chưa
            if (now < exam.StartTime)
                return ServiceResult<ExamAttempt>.IsError("Bài kiểm tra chưa đến giờ mở.");
            if (now > exam.EndTime)
                return ServiceResult<ExamAttempt>.IsError("Bài kiểm tra đã đóng.");

            // SỬA TẠI ĐÂY: Lấy lượt làm bài cũ cũng phải Include kèm cấu trúc đề thi để truyền ra View
            var existingAttempt = await _context.ExamAttempts
                .Include(a => a.Exam)
                    .ThenInclude(e => e.ExamQuestions)
                        .ThenInclude(q => q.Options)
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
                IsSubmitted = false,
                Exam = exam // SỬA TẠI ĐÂY: Gắn trực tiếp cấu trúc đề thi đã được Include vào lượt làm bài mới
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

        public async Task<ServiceResult<MyExamsVM>> GetMyExamsAsync()
        {
            string currentUserId = _httpContextAccessor.HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return ServiceResult<MyExamsVM>.IsError("Bạn chưa đăng nhập.");

            var now = DateTime.Now;

            // Truy vấn các bài kiểm tra thuộc về những lớp mà user đang ở trạng thái "STUDYING"
            var exams = await _context.Exams
                .Include(e => e.Classroom)
                // Filtered Include: Chỉ lấy Attempt của đúng user đang đăng nhập
                .Include(e => e.Attempts.Where(a => a.UserId == currentUserId))
                .Where(e => e.Classroom.ClassroomUsers.Any(cu => cu.UserId == currentUserId && cu.Status == "STUDYING"))
                .ToListAsync();

            var vm = new MyExamsVM();

            foreach (var exam in exams)
            {
                var attempt = exam.Attempts.FirstOrDefault();

                var item = new ExamItemVM
                {
                    ExamId = exam.ExamId,
                    Title = exam.Title,
                    ClassName = exam.Classroom.ClassName,
                    StartTime = exam.StartTime,
                    EndTime = exam.EndTime,
                    DurationMinutes = exam.DurationMinutes,
                    Score = attempt?.Score,
                    IsSubmitted = attempt?.IsSubmitted ?? false,
                    IsLate = attempt?.IsLate ?? false,
                };

                // SỬA LẠI ĐOẠN PHÂN LOẠI NÀY
                if (attempt == null || !attempt.IsSubmitted)
                {
                    // Tính thời gian hết hạn cá nhân (nếu đã bắt đầu làm)
                    DateTime? personalDeadline = attempt != null
                        ? attempt.StartedAt.AddMinutes(exam.DurationMinutes)
                        : null;

                    // Cho phép hiển thị ở tab "Cần làm" nếu:
                    // 1. Chưa qua Giờ đóng đề (EndTime)
                    // 2. VÀ (Chưa từng làm HOẶC chưa quá hạn thời lượng cá nhân)
                    if (now <= exam.EndTime && (personalDeadline == null || now <= personalDeadline))
                    {
                        vm.PendingExams.Add(item);
                    }
                    else
                    {
                        // Quá hạn đóng đề, hoặc hết thời lượng cá nhân mà chưa nộp -> Lỡ bài
                        item.IsMissed = true;
                        vm.CompletedExams.Add(item);
                    }
                }
                else
                {
                    // Đã nộp thành công
                    vm.CompletedExams.Add(item);
                }
            }

            // Sắp xếp tương đối: Việc cần làm gấp (EndTime gần nhất) đưa lên đầu
            vm.PendingExams = vm.PendingExams.OrderBy(x => x.EndTime).ToList();
            // Việc đã xong thì xếp mới nhất lên đầu
            vm.CompletedExams = vm.CompletedExams.OrderByDescending(x => x.EndTime).ToList();

            return ServiceResult<MyExamsVM>.IsSuccess(vm);
        }

        public async Task<ServiceResult> CreateExamFromCsvAsync(CreateExamVM model, string ownerUserId)
        {
            // 1. Kiểm tra quyền sở hữu lớp học
            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.ClassroomId == model.ClassroomId && c.OwnerUserId == ownerUserId);

            if (classroom == null)
                return ServiceResult.IsError("Bạn không có quyền tạo bài kiểm tra cho lớp học này.");

            if (model.StartTime >= model.EndTime)
                return ServiceResult.IsError("Thời gian mở phải trước thời gian đóng bài kiểm tra.");

            // 2. Khởi tạo đối tượng Exam
            var exam = new Exam
            {
                ClassroomId = model.ClassroomId,
                Title = model.Title,
                Description = model.Description,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                DurationMinutes = model.DurationMinutes
            };

            // 3. Xử lý file CSV
            var questions = new List<ExamQuestion>();
            try
            {
                using (var stream = new StreamReader(model.CsvFile.OpenReadStream()))
                {
                    bool isFirstRow = true; // Bỏ qua dòng tiêu đề (Header)
                    string line;

                    // Regex tách chuỗi theo dấu phẩy, nhưng bỏ qua các dấu phẩy nằm trong ngoặc kép ""
                    var csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

                    while ((line = await stream.ReadLineAsync()) != null)
                    {
                        if (isFirstRow)
                        {
                            isFirstRow = false;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string[] columns = csvParser.Split(line);

                        // Yêu cầu format CSV: Cột 0: Câu hỏi, 1: A, 2: B, 3: C, 4: D, 5: Đáp án đúng (A/B/C/D), 6: Giải thích
                        if (columns.Length < 6) continue; // Bỏ qua dòng lỗi

                        // Làm sạch dữ liệu (xóa dấu ngoặc kép thừa ở đầu và cuối nếu có)
                        for (int i = 0; i < columns.Length; i++)
                        {
                            columns[i] = columns[i].TrimStart('"').TrimEnd('"').Trim();
                        }

                        var correctAnswerChar = columns[5].ToUpper();

                        var question = new ExamQuestion
                        {
                            Content = columns[0],
                            Explanation = columns.Length > 6 ? columns[6] : null,
                            Options = new List<QuestionOption>
                            {
                                new QuestionOption { Content = columns[1], IsCorrect = (correctAnswerChar == "A") },
                                new QuestionOption { Content = columns[2], IsCorrect = (correctAnswerChar == "B") },
                                new QuestionOption { Content = columns[3], IsCorrect = (correctAnswerChar == "C") },
                                new QuestionOption { Content = columns[4], IsCorrect = (correctAnswerChar == "D") }
                            }
                        };
                        questions.Add(question);
                    }
                }
            }
            catch (Exception ex)
            {
                return ServiceResult.IsError($"Lỗi khi đọc file CSV: {ex.Message}");
            }

            if (!questions.Any())
                return ServiceResult.IsError("File CSV không có câu hỏi nào hợp lệ. Vui lòng kiểm tra lại định dạng.");

            exam.ExamQuestions = questions;
            _context.Exams.Add(exam);

            await _context.SaveChangesAsync();

            return ServiceResult.IsSuccess("Tạo bài kiểm tra và nhập câu hỏi thành công.");
        }
        public async Task<ServiceResult<ExamResultVM>> GetExamResultsAsync(int examId, string ownerUserId)
        {
            // 1. Lấy thông tin bài thi và kiểm tra quyền Chủ phòng
            var exam = await _context.Exams
                .Include(e => e.Classroom)
                .FirstOrDefaultAsync(e => e.ExamId == examId);

            if (exam == null)
                return ServiceResult<ExamResultVM>.IsError("Không tìm thấy bài kiểm tra.");

            if (exam.Classroom.OwnerUserId != ownerUserId)
                return ServiceResult<ExamResultVM>.IsError("Bạn không có quyền xem điểm của lớp học này.");

            var vm = new ExamResultVM
            {
                ExamId = exam.ExamId,
                Title = exam.Title,
                DurationMinutes = exam.DurationMinutes,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime
            };

            var now = DateTime.Now;

            // 2. Lấy danh sách TOÀN BỘ học sinh đang học trong lớp
            var students = await _context.ClassroomUsers
                .Include(cu => cu.User)
                .Where(cu => cu.ClassroomId == exam.ClassroomId && cu.Status == "STUDYING")
                .ToListAsync();

            // 3. Lấy TOÀN BỘ lượt làm bài của bài kiểm tra này
            var attempts = await _context.ExamAttempts
                .Where(a => a.ExamId == examId)
                .ToListAsync();

            // 4. Map dữ liệu
            foreach (var student in students)
            {
                var studentResult = new StudentResultItemVM
                {
                    UserId = student.UserId,
                    UserName = student.User.UserName
                };

                var attempt = attempts.FirstOrDefault(a => a.UserId == student.UserId);

                if (attempt == null)
                {
                    if (now > exam.EndTime)
                    {
                        studentResult.Status = "Vắng thi";
                        studentResult.StatusColor = "danger";
                        studentResult.Score = 0;
                    }
                    else
                    {
                        studentResult.Status = "Chưa làm";
                        studentResult.StatusColor = "secondary";
                    }
                }
                else
                {
                    studentResult.AttemptId = attempt.AttemptId; // BỔ SUNG DÒNG NÀY
                    studentResult.StartedAt = attempt.StartedAt;
                    studentResult.CompletedAt = attempt.CompletedAt;

                    if (attempt.IsSubmitted)
                    {
                        studentResult.Score = attempt.Score;

                        if (attempt.IsLate)
                        {
                            studentResult.Status = "Nộp trễ";
                            studentResult.StatusColor = "warning";
                        }
                        else
                        {
                            studentResult.Status = "Đã nộp";
                            studentResult.StatusColor = "success";
                        }
                    }
                    else
                    {
                        // Chưa submit
                        var personalDeadline = attempt.StartedAt.AddMinutes(exam.DurationMinutes);
                        if (now > exam.EndTime || now > personalDeadline.AddMinutes(3))
                        {
                            studentResult.Status = "Quá hạn chưa nộp";
                            studentResult.StatusColor = "danger";
                            studentResult.Score = 0;
                        }
                        else
                        {
                            studentResult.Status = "Đang làm";
                            studentResult.StatusColor = "primary";
                        }
                    }
                }

                vm.StudentResults.Add(studentResult);
            }

            // Sắp xếp: Ưu tiên hiển thị người đã nộp lên đầu, sau đó theo tên
            vm.StudentResults = vm.StudentResults
                .OrderByDescending(x => x.Score.HasValue)
                .ThenBy(x => x.UserName)
                .ToList();

            return ServiceResult<ExamResultVM>.IsSuccess(vm);
        }
        public async Task<ServiceResult<ReviewExamVM>> GetExamReviewAsync(int attemptId, string requestUserId)
        {
            var attempt = await _context.ExamAttempts
                .Include(a => a.User)
                .Include(a => a.Exam)
                    .ThenInclude(e => e.Classroom)
                .Include(a => a.Exam)
                    .ThenInclude(e => e.ExamQuestions)
                        .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId);

            if (attempt == null)
                return ServiceResult<ReviewExamVM>.IsError("Không tìm thấy bài làm.");

            // Bảo mật: Chỉ Chủ phòng mới được xem
            if (attempt.Exam.Classroom.OwnerUserId != requestUserId)
                return ServiceResult<ReviewExamVM>.IsError("Bạn không có quyền xem chi tiết bài làm này.");

            // Lấy riêng chi tiết đáp án để tránh lỗi không đồng nhất tên Navigation Property
            var attemptDetails = await _context.ExamAttemptDetails
                .Where(d => d.AttemptId == attemptId)
                .ToListAsync();

            var vm = new ReviewExamVM
            {
                ExamTitle = attempt.Exam.Title,
                StudentName = attempt.User.UserName,
                Score = attempt.Score
            };

            foreach (var q in attempt.Exam.ExamQuestions)
            {
                var detail = attemptDetails.FirstOrDefault(d => d.QuestionId == q.QuestionId);
                var selectedOptionId = detail?.SelectedOptionId;

                var questionVM = new ReviewQuestionVM
                {
                    QuestionId = q.QuestionId,
                    Content = q.Content,
                    Explanation = q.Explanation
                };

                foreach (var opt in q.Options)
                {
                    questionVM.Options.Add(new ReviewOptionVM
                    {
                        OptionId = opt.OptionId,
                        Content = opt.Content,
                        IsCorrect = opt.IsCorrect,
                        IsSelected = (opt.OptionId == selectedOptionId)
                    });
                }
                vm.Questions.Add(questionVM);
            }

            return ServiceResult<ReviewExamVM>.IsSuccess(vm);
        }
        public async Task<int> GetPendingExamsCountAsync(string userId)
        {
            var now = DateTime.Now;

            // Truy vấn các bài kiểm tra của các lớp đang học
            var exams = await _context.Exams
                .Include(e => e.Classroom)
                .Include(e => e.Attempts.Where(a => a.UserId == userId))
                .Where(e => e.Classroom.ClassroomUsers.Any(cu => cu.UserId == userId && cu.Status == "STUDYING"))
                .ToListAsync();

            int pendingCount = 0;

            foreach (var exam in exams)
            {
                var attempt = exam.Attempts.FirstOrDefault();
                if (attempt == null || !attempt.IsSubmitted)
                {
                    DateTime? personalDeadline = attempt != null ? attempt.StartedAt.AddMinutes(exam.DurationMinutes) : null;

                    // Nếu vẫn trong thời gian mở đề VÀ (chưa làm hoặc đang làm dở nhưng chưa hết giờ)
                    if (now <= exam.EndTime && (personalDeadline == null || now <= personalDeadline))
                    {
                        pendingCount++;
                    }
                }
            }

            return pendingCount;
        }
    }
}