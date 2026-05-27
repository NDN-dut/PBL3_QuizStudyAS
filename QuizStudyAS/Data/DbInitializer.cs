using QuizStudyAS.Models;
using System.Linq;
using System.Collections.Generic;
using QuizStudyAS.Services;

namespace QuizStudyAS.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context, IPasswordHasher passwordHasher)
        {
            // ==========================================
            // BƯỚC 1: ĐẢM BẢO ROLES LUÔN TỒN TẠI
            // ==========================================
            if (!context.Roles.Any(r => r.RoleName == "Admin"))
            {
                context.Roles.Add(new Role { RoleName = "Admin" });
            }
            if (!context.Roles.Any(r => r.RoleName == "User"))
            {
                context.Roles.Add(new Role { RoleName = "User" });
            }
            context.SaveChanges();

            var adminRole = context.Roles.First(r => r.RoleName == "Admin");
            var userRole = context.Roles.First(r => r.RoleName == "User");

            // ==========================================
            // BƯỚC 2: CẤY GHÉP USERS ĐỒNG BỘ MẬT KHẨU
            // ==========================================
            string defaultPassword = passwordHasher.HashPassword("123456");

            var predefinedUsers = new List<ApplicationUser>
            {
                new ApplicationUser { UserName = "admin_teacher", Email = "admin@qsas.com", RoleId = adminRole.RoleId, IsActive = true },
                new ApplicationUser { UserName = "sv_it_01", Email = "sv1@qsas.com", RoleId = userRole.RoleId, IsActive = true },
                new ApplicationUser { UserName = "sv_it_02", Email = "sv2@qsas.com", RoleId = userRole.RoleId, IsActive = true },
                new ApplicationUser { UserName = "sv_nn_03", Email = "sv3@qsas.com", RoleId = userRole.RoleId, IsActive = true },
                new ApplicationUser { UserName = "sv_it_04", Email = "sv4@qsas.com", RoleId = userRole.RoleId, IsActive = true }
            };

            var usersDict = new Dictionary<string, ApplicationUser>();

            foreach (var u in predefinedUsers)
            {
                var existingUser = context.Users.FirstOrDefault(dbU => dbU.UserName == u.UserName);
                if (existingUser == null)
                {
                    u.PasswordHash = defaultPassword;
                    context.Users.Add(u);
                    usersDict[u.UserName] = u;
                }
                else
                {
                    existingUser.PasswordHash = defaultPassword;
                    context.Users.Update(existingUser);
                    usersDict[existingUser.UserName] = existingUser;
                }
            }
            context.SaveChanges();

            // ==========================================
            // BƯỚC 3: KHỞI TẠO LỚP HỌC VÀ THÀNH VIÊN (NẾU CHƯA CÓ)
            // ==========================================
            var classrooms = new List<Classroom>();
            if (!context.Classrooms.Any(c => c.InviteCode == "GRASP2026"))
            {
                var classroomArray = new Classroom[]
                {
                    new Classroom { ClassName = "Lớp Kiến trúc Phần mềm (OOP/GRASP)", InviteCode = "GRASP2026", OwnerUserId = usersDict["admin_teacher"].Id, IsActive = true},
                    new Classroom { ClassName = "Lớp Mạng Máy Tính Cơ Bản", InviteCode = "NET2026", OwnerUserId = usersDict["admin_teacher"].Id, IsActive = true },
                    new Classroom { ClassName = "CLB Lịch sử & Chiến thuật", InviteCode = "HIS2026", OwnerUserId = usersDict["sv_it_01"].Id, IsActive = true },
                    new Classroom { ClassName = "Ôn thi JLPT N3 Cấp tốc", InviteCode = "N3PASS", OwnerUserId = usersDict["sv_nn_03"].Id, IsActive = true }
                };
                context.Classrooms.AddRange(classroomArray);
                context.SaveChanges();
                classrooms = classroomArray.ToList();

                // Khởi tạo học viên trong lớp
                var classroomUsers = new ClassroomUser[]
                {
                    new ClassroomUser { ClassroomId = classrooms[0].ClassroomId, UserId = usersDict["sv_it_01"].Id },
                    new ClassroomUser { ClassroomId = classrooms[0].ClassroomId, UserId = usersDict["sv_it_02"].Id },
                    new ClassroomUser { ClassroomId = classrooms[0].ClassroomId, UserId = usersDict["sv_it_04"].Id },
                    new ClassroomUser { ClassroomId = classrooms[1].ClassroomId, UserId = usersDict["sv_it_01"].Id },
                    new ClassroomUser { ClassroomId = classrooms[1].ClassroomId, UserId = usersDict["sv_it_04"].Id },
                    new ClassroomUser { ClassroomId = classrooms[2].ClassroomId, UserId = usersDict["sv_it_02"].Id },
                    new ClassroomUser { ClassroomId = classrooms[3].ClassroomId, UserId = usersDict["sv_it_01"].Id },
                    new ClassroomUser { ClassroomId = classrooms[3].ClassroomId, UserId = usersDict["sv_it_02"].Id }
                };
                context.ClassroomUsers.AddRange(classroomUsers);

                // Khởi tạo yêu cầu gia nhập lớp
                var listRequests = new RequestJoinClass[]
                {
                    new RequestJoinClass { ClassroomId = classrooms[0].ClassroomId, UserId = usersDict["sv_it_01"].Id, Status = "APPROVED"},
                    new RequestJoinClass { ClassroomId = classrooms[0].ClassroomId, UserId = usersDict["sv_it_02"].Id, Status = "APPROVED" },
                    new RequestJoinClass { ClassroomId = classrooms[0].ClassroomId, UserId = usersDict["sv_it_04"].Id, Status = "APPROVED" },
                    new RequestJoinClass { ClassroomId = classrooms[1].ClassroomId, UserId = usersDict["sv_it_01"].Id, Status = "APPROVED" },
                    new RequestJoinClass { ClassroomId = classrooms[1].ClassroomId, UserId = usersDict["sv_it_04"].Id, Status = "APPROVED" },
                    new RequestJoinClass { ClassroomId = classrooms[2].ClassroomId, UserId = usersDict["sv_it_02"].Id, Status = "APPROVED" },
                    new RequestJoinClass { ClassroomId = classrooms[3].ClassroomId, UserId = usersDict["sv_it_01"].Id, Status = "APPROVED" },
                    new RequestJoinClass { ClassroomId = classrooms[3].ClassroomId, UserId = usersDict["sv_it_02"].Id, Status = "APPROVED" }
                };
                context.RequestJoinClasses.AddRange(listRequests);
                context.SaveChanges();
            }
            else
            {
                classrooms = context.Classrooms.ToList();
            }

            // ==========================================
            // BƯỚC 4: KHỞI TẠO BỘ THỂ VÀ FLASHCARD (NẾU CHƯA CÓ)
            // ==========================================
            if (!context.StudySets.Any(s => s.Title == "Nguyên lý GRASP & OOP"))
            {
                var studySets = new List<StudySet>
                {
                    new StudySet { Title = "Nguyên lý GRASP & OOP", Description = "Information Expert, Creator, Controller...", OwnerUserId = usersDict["admin_teacher"].Id },
                    new StudySet { Title = "Kiến trúc .NET Core", Description = "3-Layer, Dependency Injection, Repository Pattern", OwnerUserId = usersDict["admin_teacher"].Id },
                    new StudySet { Title = "Giao thức & Subnetting", Description = "OSPF, RIP, VLSM cơ bản", OwnerUserId = usersDict["admin_teacher"].Id },
                    new StudySet { Title = "Vũ khí & Đơn vị Cổ đại", Description = "La Mã, Ottoman, Ba Lan, Đại Việt", OwnerUserId = usersDict["sv_it_01"].Id },
                    new StudySet { Title = "Từ vựng JLPT N3 - Tuần 1", Description = "Kanji và cách đọc phổ biến", OwnerUserId = usersDict["sv_nn_03"].Id }
                };
                context.StudySets.AddRange(studySets);
                context.SaveChanges();

                // Khởi tạo Flashcard chi tiết
                var flashcards = new Flashcard[]
                {
                    new Flashcard { StudySetId = studySets[0].StudySetId, Term = "Information Expert", Definition = "Gán trách nhiệm cho lớp có đủ thông tin nhất để thực hiện nó.", Example = "Lớp Order tính tổng tiền vì nó chứa danh sách OrderLine." },
                    new Flashcard { StudySetId = studySets[0].StudySetId, Term = "Pure Fabrication", Definition = "Tạo ra một lớp không có thực trong domain để giảm coupling.", Example = "Tạo lớp DatabaseHelper để xử lý kết nối DB." },
                    new Flashcard { StudySetId = studySets[0].StudySetId, Term = "Polymorphism", Definition = "Tính đa hình, cho phép xử lý các đối tượng thuộc các lớp khác nhau thông qua cùng một interface.", Example = "Method Overriding trong C++ hoặc Java." },
                    new Flashcard { StudySetId = studySets[1].StudySetId, Term = "3-Layer Architecture", Definition = "Kiến trúc 3 lớp: Presentation, Business Logic, và Data Access.", Example = "Giúp phân tách rõ ràng UI, xử lý logic và truy vấn CSDL." },
                    new Flashcard { StudySetId = studySets[1].StudySetId, Term = "Dependency Injection (DI)", Definition = "Kỹ thuật tiêm sự phụ thuộc, giúp các object không cần tự khởi tạo dependencies.", Example = "Dùng builder.Services.AddScoped() trong Program.cs." },
                    new Flashcard { StudySetId = studySets[2].StudySetId, Term = "OSPF", Definition = "Giao thức định tuyến trạng thái liên kết (Link-State).", Example = "Sử dụng thuật toán Dijkstra để tìm đường ngắn nhất." },
                    new Flashcard { StudySetId = studySets[2].StudySetId, Term = "NAT/PAT", Definition = "Kỹ thuật biên dịch địa chỉ mạng, giúp nhiều IP private dùng chung 1 IP public.", Example = "Cấu hình trên Router biên để ra Internet." },
                    new Flashcard { StudySetId = studySets[2].StudySetId, Term = "VLSM", Definition = "Variable Length Subnet Mask - Chia mạng con với subnet mask linh hoạt.", Example = "Tiết kiệm địa chỉ IP thay vì dùng classful routing." },
                    new Flashcard { StudySetId = studySets[3].StudySetId, Term = "Janissaries", Definition = "Lính ngự lâm tinh nhuệ của đế chế Ottoman.", Example = "Được trang bị súng hỏa mai sớm nhất châu Âu." },
                    new Flashcard { StudySetId = studySets[3].StudySetId, Term = "Winged Hussars", Definition = "Kỵ binh có cánh của Ba Lan.", Example = "Nổi tiếng với trận giải vây Vienna năm 1683." },
                    new Flashcard { StudySetId = studySets[3].StudySetId, Term = "Thương câu liêm", Definition = "Vũ khí cán dài có lưỡi móc ngang của quân đội phong kiến.", Example = "Chuyên dùng để móc chân ngựa của kỵ binh địch." },
                    new Flashcard { StudySetId = studySets[3].StudySetId, Term = "Testudo", Definition = "Đội hình mai rùa nổi tiếng của bộ binh La Mã.", Example = "Dùng khiên che kín đầu và xung quanh để chống tên bắn." },
                    new Flashcard { StudySetId = studySets[4].StudySetId, Term = "条件 (Jouken)", Definition = "Điều kiện", Example = "条件を満たす (Thỏa mãn điều kiện)" },
                    new Flashcard { StudySetId = studySets[4].StudySetId, Term = "技術 (Gijutsu)", Definition = "Kỹ thuật, công nghệ", Example = "IT技術u học công nghệ IT)" },
                    new Flashcard { StudySetId = studySets[4].StudySetId, Term = "経験 (Keiken)", Definition = "Kinh nghiệm", Example = "経験を積む (Tích lũy kinh nghiệm)" },
                    new Flashcard { StudySetId = studySets[4].StudySetId, Term = "準備 (Junbi)", Definition = "Chuẩn bị", Example = "試験の準備をする (Chuẩn bị cho kỳ thi)" }
                };
                context.Flashcards.AddRange(flashcards);
                context.SaveChanges();

                // Khởi tạo Tiến độ học tập mẫu
                var progresses = new LearningProgress[]
                {
                    new LearningProgress { UserId = usersDict["sv_it_01"].Id, FlashcardId = flashcards[0].FlashcardId, IsMastered = true, WrongCount = 0 },
                    new LearningProgress { UserId = usersDict["sv_it_01"].Id, FlashcardId = flashcards[1].FlashcardId, IsMastered = false, WrongCount = 3 },
                    new LearningProgress { UserId = usersDict["sv_it_01"].Id, FlashcardId = flashcards[12].FlashcardId, IsMastered = true, WrongCount = 1 }
                };
                context.LearningProgresses.AddRange(progresses);

                // Khởi tạo Lịch sử Mini-Game mẫu
                var gameSession = new GameSession
                {
                    UserId = usersDict["sv_it_01"].Id,
                    StudySetId = studySets[0].StudySetId,
                    GameType = 1,
                    Score = 80,
                    CompletionTime = 120
                };
                context.GameSessions.Add(gameSession);
                context.SaveChanges();

                var quizResults = new QuizQuestionResult[]
                {
                    new QuizQuestionResult { SessionId = gameSession.SessionId, FlashcardId = flashcards[0].FlashcardId, IsCorrect = true },
                    new QuizQuestionResult { SessionId = gameSession.SessionId, FlashcardId = flashcards[1].FlashcardId, IsCorrect = false },
                    new QuizQuestionResult { SessionId = gameSession.SessionId, FlashcardId = flashcards[2].FlashcardId, IsCorrect = true }
                };
                context.QuizQuestionResults.AddRange(quizResults);
                context.SaveChanges();

                // Khởi tạo Tài liệu lớp học (Classroom Materials) kết nối các lớp hiện có
                if (classrooms.Count >= 4)
                {
                    var materials = new ClassRoomMaterial[]
                    {
                        new ClassRoomMaterial { ClassRoomId = classrooms[0].ClassroomId, StudySetId = studySets[0].StudySetId, Status = "AVAILABLE" },
                        new ClassRoomMaterial { ClassRoomId = classrooms[0].ClassroomId, StudySetId = studySets[1].StudySetId, Status = "AVAILABLE" },
                        new ClassRoomMaterial { ClassRoomId = classrooms[1].ClassroomId, StudySetId = studySets[2].StudySetId, Status = "AVAILABLE" },
                        new ClassRoomMaterial { ClassRoomId = classrooms[2].ClassroomId, StudySetId = studySets[3].StudySetId, Status = "AVAILABLE" },
                        new ClassRoomMaterial { ClassRoomId = classrooms[3].ClassroomId, StudySetId = studySets[4].StudySetId, Status = "AVAILABLE" }
                    };
                    context.ClassRoomMaterials.AddRange(materials);
                    context.SaveChanges();
                }
            }
        }
    }
}