using QuizStudyAS.Models;

namespace QuizStudyAS.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Kiểm tra xem Database đã có dữ liệu chưa. 
            // Nếu có ít nhất 1 User thì không cần nạp lại nữa.
            if (context.Users.Any())
            {
                return;
            }

            // --- 1. KHỞI TẠO USER ---
            // Nên gọi SaveChanges() sau mỗi bước để EF Core tự sinh ID (Khóa chính) 
            // dùng cho các bảng phía sau.
            var users = new ApplicationUser[]
            {
                new ApplicationUser { UserName = "giaovien1", Email = "gv@qsas.com", PasswordHash = "hashed_pass_1" },
                new ApplicationUser { UserName = "sinhvien1", Email = "sv@qsas.com", PasswordHash = "hashed_pass_2" }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // --- 2. KHỞI TẠO LỚP HỌC (Classroom) ---
            var classrooms = new Classroom[]
            {
                new Classroom { ClassName = "Lớp học OOP .NET", InviteCode = "OOP2026", OwnerUserId = users[0].Id }
            };
            context.Classrooms.AddRange(classrooms);
            context.SaveChanges();

            // --- 3. KHỞI TẠO HỌC VIÊN TRONG LỚP (ClassroomUser) ---
            var classroomUsers = new ClassroomUser[]
            {
                // Sinh viên 1 tham gia lớp của Giáo viên 1
                new ClassroomUser { ClassroomId = classrooms[0].ClassroomId, UserId = users[1].Id }
            };
            context.ClassroomUsers.AddRange(classroomUsers);
            context.SaveChanges();

            // --- 4. KHỞI TẠO BỘ THẺ (StudySet) ---
            var studySets = new StudySet[]
            {
                // Bộ thẻ này thuộc lớp học OOP
                new StudySet { Title = "Từ vựng Hướng đối tượng", Description = "Các khái niệm cơ bản", OwnerUserId = users[0].Id, ClassroomId = classrooms[0].ClassroomId },
                // Bộ thẻ cá nhân, không thuộc lớp nào
                new StudySet { Title = "Từ vựng JLPT N3", Description = "Ôn tập kanji", OwnerUserId = users[1].Id }
            };
            context.StudySets.AddRange(studySets);
            context.SaveChanges();

            // --- 5. KHỞI TẠO FLASHCARD ---
            var flashcards = new Flashcard[]
            {
                // Flashcard cho bộ OOP
                new Flashcard { StudySetId = studySets[0].StudySetId, Term = "Encapsulation", Definition = "Tính đóng gói", Example = "Dùng private field và public property" },
                new Flashcard { StudySetId = studySets[0].StudySetId, Term = "Polymorphism", Definition = "Tính đa hình", Example = "Method Overriding" },
                
                // Flashcard cho bộ JLPT N3
                new Flashcard { StudySetId = studySets[1].StudySetId, Term = "食べる", Definition = "Ăn (Taberu)", Example = "ご飯を食べる" }
            };
            context.Flashcards.AddRange(flashcards);
            context.SaveChanges();

            // --- 6. KHỞI TẠO TIẾN ĐỘ HỌC (LearningProgress) ---
            var progresses = new LearningProgress[]
            {
                // Sinh viên 1 chưa thuộc từ Encapsulation (sai 2 lần)
                new LearningProgress { UserId = users[1].Id, FlashcardId = flashcards[0].FlashcardId, IsMastered = false, WrongCount = 2 }
            };
            context.LearningProgresses.AddRange(progresses);
            context.SaveChanges();

            // --- 7. KHỞI TẠO KẾT QUẢ MINI-GAME (GameSession & QuizQuestionResult) ---
            var session = new GameSession
            {
                UserId = users[1].Id,
                StudySetId = studySets[0].StudySetId,
                GameType = 1, // 1 là trắc nghiệm
                Score = 50,
                CompletionTime = 120
            };
            context.GameSessions.Add(session);
            context.SaveChanges(); // Phải lưu session trước để lấy SessionId

            var quizResults = new QuizQuestionResult[]
            {
                new QuizQuestionResult { SessionId = session.SessionId, FlashcardId = flashcards[0].FlashcardId, IsCorrect = false },
                new QuizQuestionResult { SessionId = session.SessionId, FlashcardId = flashcards[1].FlashcardId, IsCorrect = true }
            };
            context.QuizQuestionResults.AddRange(quizResults);
            context.SaveChanges();
        }
    }
}