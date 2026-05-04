using QuizStudyAS.Models;
using BCrypt.Net;

namespace QuizStudyAS.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Nếu có dữ liệu rồi thì bỏ qua
            if (context.Users.Any()) return;

            // --- 0. KHỞI TẠO ROLE ---
            var roles = new Role[]
            {
                new Role { RoleName = "Admin" }, // RoleId = 1
                new Role { RoleName = "User" }   // RoleId = 2
            };
            context.Roles.AddRange(roles);
            context.SaveChanges();

            // --- 1. KHỞI TẠO USER VỚI BCRYPT ---
            // Tự động sinh Salt và băm mật khẩu mặc định cho tất cả tài khoản test
            string defaultPassword = BCrypt.Net.BCrypt.HashPassword("123456");

            var users = new ApplicationUser[]
            {
                new ApplicationUser { UserName = "admin_teacher", Email = "admin@qsas.com", PasswordHash = defaultPassword, RoleId = roles[0].RoleId },
                new ApplicationUser { UserName = "sv_it_01", Email = "sv1@qsas.com", PasswordHash = defaultPassword, RoleId = roles[1].RoleId },
                new ApplicationUser { UserName = "sv_it_02", Email = "sv2@qsas.com", PasswordHash = defaultPassword, RoleId = roles[1].RoleId },
                new ApplicationUser { UserName = "sv_nn_03", Email = "sv3@qsas.com", PasswordHash = defaultPassword, RoleId = roles[1].RoleId },
                new ApplicationUser { UserName = "sv_it_04", Email = "sv4@qsas.com", PasswordHash = defaultPassword, RoleId = roles[1].RoleId }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // --- 2. KHỞI TẠO LỚP HỌC (Classroom) ---
            var classrooms = new Classroom[]
            {
                new Classroom { ClassName = "Lớp Kiến trúc Phần mềm (OOP/GRASP)", InviteCode = "GRASP2026", OwnerUserId = users[0].Id },
                new Classroom { ClassName = "Lớp Mạng Máy Tính Cơ Bản", InviteCode = "NET2026", OwnerUserId = users[0].Id },
                new Classroom { ClassName = "CLB Lịch sử & Chiến thuật", InviteCode = "HIS2026", OwnerUserId = users[1].Id },
                new Classroom { ClassName = "Ôn thi JLPT N3 Cấp tốc", InviteCode = "N3PASS", OwnerUserId = users[3].Id }
            };
            context.Classrooms.AddRange(classrooms);
            context.SaveChanges();

            // --- 3. KHỞI TẠO HỌC VIÊN TRONG LỚP ---
            var classroomUsers = new ClassroomUser[]
            {
                new ClassroomUser { ClassroomId = classrooms[0].ClassroomId, UserId = users[1].Id },
                new ClassroomUser { ClassroomId = classrooms[0].ClassroomId, UserId = users[2].Id },
                new ClassroomUser { ClassroomId = classrooms[0].ClassroomId, UserId = users[4].Id },
                new ClassroomUser { ClassroomId = classrooms[1].ClassroomId, UserId = users[1].Id },
                new ClassroomUser { ClassroomId = classrooms[1].ClassroomId, UserId = users[4].Id },
                new ClassroomUser { ClassroomId = classrooms[2].ClassroomId, UserId = users[2].Id },
                new ClassroomUser { ClassroomId = classrooms[3].ClassroomId, UserId = users[1].Id }, // SV IT cũng học tiếng Nhật
                new ClassroomUser { ClassroomId = classrooms[3].ClassroomId, UserId = users[2].Id }
            };
            context.ClassroomUsers.AddRange(classroomUsers);
            context.SaveChanges();

            // --- 4. KHỞI TẠO BỘ THẺ (StudySet) ---
            var studySets = new StudySet[]
            {
                new StudySet { Title = "Nguyên lý GRASP & OOP", Description = "Information Expert, Creator, Controller...", OwnerUserId = users[0].Id, ClassroomId = classrooms[0].ClassroomId },
                new StudySet { Title = "Kiến trúc .NET Core", Description = "3-Layer, Dependency Injection, Repository Pattern", OwnerUserId = users[0].Id, ClassroomId = classrooms[0].ClassroomId },
                new StudySet { Title = "Giao thức & Subnetting", Description = "OSPF, RIP, VLSM cơ bản", OwnerUserId = users[0].Id, ClassroomId = classrooms[1].ClassroomId },
                new StudySet { Title = "Vũ khí & Đơn vị Cổ đại", Description = "La Mã, Ottoman, Ba Lan, Đại Việt", OwnerUserId = users[1].Id, ClassroomId = classrooms[2].ClassroomId },
                new StudySet { Title = "Từ vựng JLPT N3 - Tuần 1", Description = "Kanji và cách đọc phổ biến", OwnerUserId = users[3].Id, ClassroomId = classrooms[3].ClassroomId }
            };
            context.StudySets.AddRange(studySets);
            context.SaveChanges();

            // --- 5. KHỞI TẠO FLASHCARD ---
            var flashcards = new Flashcard[]
            {
                // Set 1: GRASP & OOP
                new Flashcard { StudySetId = studySets[0].StudySetId, Term = "Information Expert", Definition = "Gán trách nhiệm cho lớp có đủ thông tin nhất để thực hiện nó.", Example = "Lớp Order tính tổng tiền vì nó chứa danh sách OrderLine." },
                new Flashcard { StudySetId = studySets[0].StudySetId, Term = "Pure Fabrication", Definition = "Tạo ra một lớp không có thực trong domain để giảm coupling.", Example = "Tạo lớp DatabaseHelper để xử lý kết nối DB." },
                new Flashcard { StudySetId = studySets[0].StudySetId, Term = "Polymorphism", Definition = "Tính đa hình, cho phép xử lý các đối tượng thuộc các lớp khác nhau thông qua cùng một interface.", Example = "Method Overriding trong C++ hoặc Java." },
                
                // Set 2: .NET Core
                new Flashcard { StudySetId = studySets[1].StudySetId, Term = "3-Layer Architecture", Definition = "Kiến trúc 3 lớp: Presentation, Business Logic, và Data Access.", Example = "Giúp phân tách rõ ràng UI, xử lý logic và truy vấn CSDL." },
                new Flashcard { StudySetId = studySets[1].StudySetId, Term = "Dependency Injection (DI)", Definition = "Kỹ thuật tiêm sự phụ thuộc, giúp các object không cần tự khởi tạo dependencies.", Example = "Dùng builder.Services.AddScoped() trong Program.cs." },

                // Set 3: Networking
                new Flashcard { StudySetId = studySets[2].StudySetId, Term = "OSPF", Definition = "Giao thức định tuyến trạng thái liên kết (Link-State).", Example = "Sử dụng thuật toán Dijkstra để tìm đường ngắn nhất." },
                new Flashcard { StudySetId = studySets[2].StudySetId, Term = "NAT/PAT", Definition = "Kỹ thuật biên dịch địa chỉ mạng, giúp nhiều IP private dùng chung 1 IP public.", Example = "Cấu hình trên Router biên để ra Internet." },
                new Flashcard { StudySetId = studySets[2].StudySetId, Term = "VLSM", Definition = "Variable Length Subnet Mask - Chia mạng con với subnet mask linh hoạt.", Example = "Tiết kiệm địa chỉ IP thay vì dùng classful routing." },

                // Set 4: History
                new Flashcard { StudySetId = studySets[3].StudySetId, Term = "Janissaries", Definition = "Lính ngự lâm tinh nhuệ của đế chế Ottoman.", Example = "Được trang bị súng hỏa mai sớm nhất châu Âu." },
                new Flashcard { StudySetId = studySets[3].StudySetId, Term = "Winged Hussars", Definition = "Kỵ binh có cánh của Ba Lan.", Example = "Nổi tiếng với trận giải vây Vienna năm 1683." },
                new Flashcard { StudySetId = studySets[3].StudySetId, Term = "Thương câu liêm", Definition = "Vũ khí cán dài có lưỡi móc ngang của quân đội phong kiến.", Example = "Chuyên dùng để móc chân ngựa của kỵ binh địch." },
                new Flashcard { StudySetId = studySets[3].StudySetId, Term = "Testudo", Definition = "Đội hình mai rùa nổi tiếng của bộ binh La Mã.", Example = "Dùng khiên che kín đầu và xung quanh để chống tên bắn." },

                // Set 5: JLPT N3
                new Flashcard { StudySetId = studySets[4].StudySetId, Term = "条件 (Jouken)", Definition = "Điều kiện", Example = "条件を満たす (Thỏa mãn điều kiện)" },
                new Flashcard { StudySetId = studySets[4].StudySetId, Term = "技術 (Gijutsu)", Definition = "Kỹ thuật, công nghệ", Example = "IT技術を学ぶ (Học công nghệ IT)" },
                new Flashcard { StudySetId = studySets[4].StudySetId, Term = "経験 (Keiken)", Definition = "Kinh nghiệm", Example = "経験を積む (Tích lũy kinh nghiệm)" },
                new Flashcard { StudySetId = studySets[4].StudySetId, Term = "準備 (Junbi)", Definition = "Chuẩn bị", Example = "試験の準備をする (Chuẩn bị cho kỳ thi)" }
            };
            context.Flashcards.AddRange(flashcards);
            context.SaveChanges();

            // --- 6. KHỞI TẠO TIẾN ĐỘ HỌC ---
            var progresses = new LearningProgress[]
            {
                new LearningProgress { UserId = users[1].Id, FlashcardId = flashcards[0].FlashcardId, IsMastered = true, WrongCount = 0 },
                new LearningProgress { UserId = users[1].Id, FlashcardId = flashcards[1].FlashcardId, IsMastered = false, WrongCount = 3 },
                new LearningProgress { UserId = users[1].Id, FlashcardId = flashcards[13].FlashcardId, IsMastered = true, WrongCount = 1 }
            };
            context.LearningProgresses.AddRange(progresses);
            context.SaveChanges();

            // --- 7. KHỞI TẠO KẾT QUẢ MINI-GAME ---
            var session = new GameSession
            {
                UserId = users[1].Id,
                StudySetId = studySets[0].StudySetId,
                GameType = 1,
                Score = 80,
                CompletionTime = 120
            };
            context.GameSessions.Add(session);
            context.SaveChanges();

            var quizResults = new QuizQuestionResult[]
            {
                new QuizQuestionResult { SessionId = session.SessionId, FlashcardId = flashcards[0].FlashcardId, IsCorrect = true },
                new QuizQuestionResult { SessionId = session.SessionId, FlashcardId = flashcards[1].FlashcardId, IsCorrect = false },
                new QuizQuestionResult { SessionId = session.SessionId, FlashcardId = flashcards[2].FlashcardId, IsCorrect = true }
            };
            context.QuizQuestionResults.AddRange(quizResults);
            context.SaveChanges();
        }
    }
}