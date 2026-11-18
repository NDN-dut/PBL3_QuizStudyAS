using QuizStudyAS.Constants;
using QuizStudyAS.Models;
using System.Linq;
using System.Collections.Generic;
using QuizStudyAS.Services.Auth;

namespace QuizStudyAS.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context, IPasswordHasher passwordHasher)
        {
            if (!context.AuthProviders.Any())
            {
                var providers = new List<AuthProvider>
                {
                    new AuthProvider { Id = 1, Name = "Local" },
                    new AuthProvider { Id = 2, Name = "Google" }
                };
                context.AuthProviders.AddRange(providers);
                context.SaveChanges(); // Lưu trước để phía dưới User có Id để tham chiếu
            }
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
                new ApplicationUser { UserName = "admin_teacher", Email = "admin@qsas.com", RoleId = adminRole.RoleId, IsActive = true, AuthProviderId = 1 },
                new ApplicationUser { UserName = "sv_it_01", Email = "sv1@qsas.com", RoleId = userRole.RoleId, IsActive = true, AuthProviderId = 1 },
                new ApplicationUser { UserName = "sv_it_02", Email = "sv2@qsas.com", RoleId = userRole.RoleId, IsActive = true, AuthProviderId = 1 },
                new ApplicationUser { UserName = "sv_nn_03", Email = "sv3@qsas.com", RoleId = userRole.RoleId, IsActive = true, AuthProviderId = 1 },
                new ApplicationUser { UserName = "sv_it_04", Email = "sv4@qsas.com", RoleId = userRole.RoleId, IsActive = true, AuthProviderId = 1 },
                // Tài khoản dùng cho video demo phỏng vấn: giáo viên tạo lớp, học sinh tìm và xin vào lớp
                new ApplicationUser { UserName = "demo_teacher", Email = "teacher@qsas.com", RoleId = userRole.RoleId, IsActive = true, AuthProviderId = 1 },
                new ApplicationUser { UserName = "demo_student", Email = "student@qsas.com", RoleId = userRole.RoleId, IsActive = true, AuthProviderId = 1 }
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

            // Máy nào đã có dữ liệu mẫu tiếng Việt cũ thì đổi sang tiếng Anh (chỉ đổi bản ghi chưa bị sửa)
            TranslateLegacySeedData(context);

            // ==========================================
            // BƯỚC 3: KHỞI TẠO LỚP HỌC VÀ THÀNH VIÊN (NẾU CHƯA CÓ)
            // ==========================================
            var classrooms = new List<Classroom>();
            if (!context.Classrooms.Any(c => c.InviteCode == "GRASP2026"))
            {
                var classroomArray = new Classroom[]
                {
                    new Classroom { ClassName = "Software Architecture (OOP & GRASP)", InviteCode = "GRASP2026", OwnerUserId = usersDict["admin_teacher"].Id, StatusId = 1},
                    new Classroom { ClassName = "Computer Networks Basics", InviteCode = "NET2026", OwnerUserId = usersDict["admin_teacher"].Id, StatusId = 1 },
                    new Classroom { ClassName = "World History & Strategy Club", InviteCode = "HIS2026", OwnerUserId = usersDict["sv_it_01"].Id, StatusId = 1 },
                    new Classroom { ClassName = "JLPT N3 Intensive Course", InviteCode = "N3PASS", OwnerUserId = usersDict["sv_nn_03"].Id, StatusId = 1 }
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
            if (!context.StudySets.Any(s => s.Title == "GRASP & OOP Principles" || s.Title == "Nguyên lý GRASP & OOP"))
            {
                var studySets = new List<StudySet>
                {
                    new StudySet { Title = "GRASP & OOP Principles", Description = "Information Expert, Creator, Controller...", OwnerUserId = usersDict["admin_teacher"].Id },
                    new StudySet { Title = ".NET Core Architecture", Description = "3-Layer, Dependency Injection, Repository Pattern", OwnerUserId = usersDict["admin_teacher"].Id },
                    new StudySet { Title = "Network Protocols & Subnetting", Description = "OSPF, RIP and VLSM basics", OwnerUserId = usersDict["admin_teacher"].Id },
                    new StudySet { Title = "Ancient Weapons & Units", Description = "Rome, Ottoman Empire, Poland, Dai Viet", OwnerUserId = usersDict["sv_it_01"].Id },
                    new StudySet { Title = "JLPT N3 Vocabulary - Week 1", Description = "Common kanji and their readings", OwnerUserId = usersDict["sv_nn_03"].Id }
                };
                context.StudySets.AddRange(studySets);
                context.SaveChanges();

                // Khởi tạo Flashcard chi tiết
                var flashcards = new Flashcard[]
                {
                    new Flashcard { StudySetId = studySets[0].StudySetId, Term = "Information Expert", Definition = "Assign a responsibility to the class that has the information needed to fulfill it.", Example = "Order calculates its total because it holds the list of OrderLines." },
                    new Flashcard { StudySetId = studySets[0].StudySetId, Term = "Pure Fabrication", Definition = "Create a class that does not exist in the problem domain to reduce coupling.", Example = "A DatabaseHelper class that manages database connections." },
                    new Flashcard { StudySetId = studySets[0].StudySetId, Term = "Polymorphism", Definition = "Handle objects of different classes through the same interface.", Example = "Method overriding in C++ or Java." },
                    new Flashcard { StudySetId = studySets[1].StudySetId, Term = "3-Layer Architecture", Definition = "Splits an application into Presentation, Business Logic and Data Access layers.", Example = "Keeps UI, business rules and database queries separate." },
                    new Flashcard { StudySetId = studySets[1].StudySetId, Term = "Dependency Injection (DI)", Definition = "Objects receive their dependencies instead of creating them themselves.", Example = "builder.Services.AddScoped() in Program.cs." },
                    new Flashcard { StudySetId = studySets[2].StudySetId, Term = "OSPF", Definition = "A link-state routing protocol.", Example = "Uses Dijkstra's algorithm to find the shortest path." },
                    new Flashcard { StudySetId = studySets[2].StudySetId, Term = "NAT/PAT", Definition = "Translates private IP addresses so many hosts can share one public IP.", Example = "Configured on the edge router to reach the Internet." },
                    new Flashcard { StudySetId = studySets[2].StudySetId, Term = "VLSM", Definition = "Variable Length Subnet Mask - subnetting with flexible mask sizes.", Example = "Saves IP addresses compared with classful routing." },
                    new Flashcard { StudySetId = studySets[3].StudySetId, Term = "Janissaries", Definition = "Elite infantry units of the Ottoman Empire.", Example = "Among the first soldiers in Europe to use muskets." },
                    new Flashcard { StudySetId = studySets[3].StudySetId, Term = "Winged Hussars", Definition = "Polish heavy cavalry famous for the wing-like frames on their backs.", Example = "Famous for the relief of Vienna in 1683." },
                    new Flashcard { StudySetId = studySets[3].StudySetId, Term = "Hook Spear (Câu liêm)", Definition = "A long-handled weapon with a side hook used by Vietnamese armies.", Example = "Used to hook the legs of enemy cavalry horses." },
                    new Flashcard { StudySetId = studySets[3].StudySetId, Term = "Testudo", Definition = "The Roman 'tortoise' shield formation.", Example = "Shields covered the front and top of the unit against arrows." },
                    new Flashcard { StudySetId = studySets[4].StudySetId, Term = "条件 (Jouken)", Definition = "Condition", Example = "条件を満たす (to meet the conditions)" },
                    new Flashcard { StudySetId = studySets[4].StudySetId, Term = "技術 (Gijutsu)", Definition = "Technology, technique", Example = "IT技術を学ぶ (to learn IT technology)" },
                    new Flashcard { StudySetId = studySets[4].StudySetId, Term = "経験 (Keiken)", Definition = "Experience", Example = "経験を積む (to gain experience)" },
                    new Flashcard { StudySetId = studySets[4].StudySetId, Term = "準備 (Junbi)", Definition = "Preparation", Example = "試験の準備をする (to prepare for an exam)" }
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

            // Dữ liệu mẫu tiếng Anh cho video demo (10 lớp học, flashcard, bảng xếp hạng)
            SeedEnglishDemoData(context, usersDict);
        }

        // ==========================================
        // DỮ LIỆU MẪU TIẾNG ANH CHO VIDEO DEMO
        // ==========================================

        // Đổi dữ liệu mẫu tiếng Việt cũ (đã có sẵn trong DB của các máy đang chạy) sang tiếng Anh.
        // Chỉ đổi bản ghi còn giữ nguyên tên tiếng Việt gốc nên chạy lại nhiều lần vẫn an toàn.
        private static void TranslateLegacySeedData(AppDbContext context)
        {
            var classNames = new Dictionary<string, (string OldName, string NewName)>
            {
                ["GRASP2026"] = ("Lớp Kiến trúc Phần mềm (OOP/GRASP)", "Software Architecture (OOP & GRASP)"),
                ["NET2026"] = ("Lớp Mạng Máy Tính Cơ Bản", "Computer Networks Basics"),
                ["HIS2026"] = ("CLB Lịch sử & Chiến thuật", "World History & Strategy Club"),
                ["N3PASS"] = ("Ôn thi JLPT N3 Cấp tốc", "JLPT N3 Intensive Course"),
            };
            foreach (var (inviteCode, names) in classNames)
            {
                var classroom = context.Classrooms.FirstOrDefault(c => c.InviteCode == inviteCode && c.ClassName == names.OldName);
                if (classroom != null)
                {
                    classroom.ClassName = names.NewName;
                }
            }

            var studySets = new Dictionary<string, (string Title, string Description)>
            {
                ["Nguyên lý GRASP & OOP"] = ("GRASP & OOP Principles", "Information Expert, Creator, Controller..."),
                ["Kiến trúc .NET Core"] = (".NET Core Architecture", "3-Layer, Dependency Injection, Repository Pattern"),
                ["Giao thức & Subnetting"] = ("Network Protocols & Subnetting", "OSPF, RIP and VLSM basics"),
                ["Vũ khí & Đơn vị Cổ đại"] = ("Ancient Weapons & Units", "Rome, Ottoman Empire, Poland, Dai Viet"),
                ["Từ vựng JLPT N3 - Tuần 1"] = ("JLPT N3 Vocabulary - Week 1", "Common kanji and their readings"),
            };
            var flashcards = new Dictionary<string, (string Term, string Definition, string Example)>
            {
                ["Information Expert"] = ("Information Expert", "Assign a responsibility to the class that has the information needed to fulfill it.", "Order calculates its total because it holds the list of OrderLines."),
                ["Pure Fabrication"] = ("Pure Fabrication", "Create a class that does not exist in the problem domain to reduce coupling.", "A DatabaseHelper class that manages database connections."),
                ["Polymorphism"] = ("Polymorphism", "Handle objects of different classes through the same interface.", "Method overriding in C++ or Java."),
                ["3-Layer Architecture"] = ("3-Layer Architecture", "Splits an application into Presentation, Business Logic and Data Access layers.", "Keeps UI, business rules and database queries separate."),
                ["Dependency Injection (DI)"] = ("Dependency Injection (DI)", "Objects receive their dependencies instead of creating them themselves.", "builder.Services.AddScoped() in Program.cs."),
                ["OSPF"] = ("OSPF", "A link-state routing protocol.", "Uses Dijkstra's algorithm to find the shortest path."),
                ["NAT/PAT"] = ("NAT/PAT", "Translates private IP addresses so many hosts can share one public IP.", "Configured on the edge router to reach the Internet."),
                ["VLSM"] = ("VLSM", "Variable Length Subnet Mask - subnetting with flexible mask sizes.", "Saves IP addresses compared with classful routing."),
                ["Janissaries"] = ("Janissaries", "Elite infantry units of the Ottoman Empire.", "Among the first soldiers in Europe to use muskets."),
                ["Winged Hussars"] = ("Winged Hussars", "Polish heavy cavalry famous for the wing-like frames on their backs.", "Famous for the relief of Vienna in 1683."),
                ["Thương câu liêm"] = ("Hook Spear (Câu liêm)", "A long-handled weapon with a side hook used by Vietnamese armies.", "Used to hook the legs of enemy cavalry horses."),
                ["Testudo"] = ("Testudo", "The Roman 'tortoise' shield formation.", "Shields covered the front and top of the unit against arrows."),
                ["条件 (Jouken)"] = ("条件 (Jouken)", "Condition", "条件を満たす (to meet the conditions)"),
                ["技術 (Gijutsu)"] = ("技術 (Gijutsu)", "Technology, technique", "IT技術を学ぶ (to learn IT technology)"),
                ["経験 (Keiken)"] = ("経験 (Keiken)", "Experience", "経験を積む (to gain experience)"),
                ["準備 (Junbi)"] = ("準備 (Junbi)", "Preparation", "試験の準備をする (to prepare for an exam)"),
            };
            foreach (var (oldTitle, english) in studySets)
            {
                var studySet = context.StudySets.FirstOrDefault(s => s.Title == oldTitle);
                if (studySet == null) continue;

                studySet.Title = english.Title;
                studySet.Description = english.Description;

                foreach (var card in context.Flashcards.Where(f => f.StudySetId == studySet.StudySetId).ToList())
                {
                    if (flashcards.TryGetValue(card.Term, out var translated))
                    {
                        card.Term = translated.Term;
                        card.Definition = translated.Definition;
                        card.Example = translated.Example;
                    }
                }
            }
            context.SaveChanges();
        }

        // Thêm 6 lớp học + 5 bộ flashcard tiếng Anh (tổng cộng 10 lớp) và điểm cho bảng xếp hạng.
        // Mỗi phần đều kiểm tra "đã có chưa" trước khi thêm nên không bị nhân bản khi khởi động lại.
        private static void SeedEnglishDemoData(AppDbContext context, Dictionary<string, ApplicationUser> users)
        {
            // 1) Lớp học: đặt tên gần giống nhau (Java / JavaScript / Programming...) để thấy rõ tìm kiếm mờ
            var demoClasses = new (string Code, string Name, string Owner, string[] Members)[]
            {
                ("JAVA201", "Advanced Java Programming", "sv_it_01", new[] { "sv_it_02", "sv_it_04" }),
                ("JS101", "JavaScript for Beginners", "sv_it_04", new[] { "sv_it_01" }),
                ("PY101", "Python Programming Basics", "admin_teacher", new[] { "sv_it_02", "sv_nn_03" }),
                ("DSA101", "Data Structures and Algorithms", "admin_teacher", new[] { "sv_it_01", "sv_it_02", "sv_it_04" }),
                ("SQL101", "Database Design with SQL Server", "sv_nn_03", new[] { "sv_it_01" }),
                ("JPIT01", "Japanese for IT Engineers", "sv_nn_03", new[] { "sv_it_02", "sv_it_04", "demo_student" })
            };
            foreach (var demo in demoClasses)
            {
                if (context.Classrooms.Any(c => c.InviteCode == demo.Code)) continue;

                var classroom = new Classroom
                {
                    ClassName = demo.Name,
                    InviteCode = demo.Code,
                    OwnerUserId = users[demo.Owner].Id,
                    StatusId = (int)ClassroomStatusEnum.Active
                };
                context.Classrooms.Add(classroom);
                context.SaveChanges();

                foreach (var member in demo.Members)
                {
                    context.ClassroomUsers.Add(new ClassroomUser { ClassroomId = classroom.ClassroomId, UserId = users[member].Id });
                    context.RequestJoinClasses.Add(new RequestJoinClass { ClassroomId = classroom.ClassroomId, UserId = users[member].Id, Status = RequestJoinStatus.Approved });
                }
                context.SaveChanges();
            }

            // 2) Bộ flashcard: "Java Basics" của demo_teacher để chia sẻ vào lớp khi quay demo,
            //    "IT Japanese Vocabulary" của demo_student để học thử flashcard / quiz
            var demoSets = new (string Title, string Description, string Owner, string? ClassCode, (string Term, string Definition, string Example)[] Cards)[]
            {
                ("Java Basics", "Core object-oriented concepts in Java", "demo_teacher", null, new[]
                {
                    ("Class", "A blueprint that defines the fields and methods of its objects.", "class Student { String name; }"),
                    ("Object", "An instance of a class created at runtime.", "Student s = new Student();"),
                    ("Inheritance", "A class reuses and extends the behavior of another class.", "class Dog extends Animal"),
                    ("Interface", "A contract listing the methods a class must implement.", "class Car implements Drivable"),
                    ("Exception", "An error event that can be handled with try/catch.", "try { ... } catch (IOException e) { ... }")
                }),
                ("IT Japanese Vocabulary", "Japanese words used every day in IT projects", "demo_student", null, new[]
                {
                    ("データベース (deetabeesu)", "Database", "データベースを設計する (to design a database)"),
                    ("要件定義 (youken teigi)", "Requirements definition", "要件定義書を作成する (to write a requirements document)"),
                    ("設計書 (sekkeisho)", "Design document", "設計書をレビューする (to review the design document)"),
                    ("不具合 (fuguai)", "Bug, defect", "不具合を修正する (to fix a bug)"),
                    ("本番環境 (honban kankyou)", "Production environment", "本番環境にリリースする (to release to production)"),
                    ("仕様 (shiyou)", "Specification", "仕様を確認する (to check the specification)"),
                    ("納期 (nouki)", "Delivery deadline", "納期に間に合わせる (to meet the deadline)"),
                    ("運用保守 (unyou hoshu)", "Operation and maintenance", "システムの運用保守を担当する (to be in charge of system operation and maintenance)")
                }),
                ("SQL Essentials", "Keys, joins, indexes and normalization", "sv_nn_03", "SQL101", new[]
                {
                    ("Primary Key", "A column that uniquely identifies each row in a table.", "UserId INT PRIMARY KEY"),
                    ("Foreign Key", "A column that references the primary key of another table.", "Orders.UserId references Users.UserId"),
                    ("JOIN", "Combines rows from two or more tables based on a related column.", "SELECT * FROM Orders JOIN Users ON Orders.UserId = Users.UserId"),
                    ("Index", "Speeds up lookups at the cost of extra storage and slower writes.", "CREATE INDEX IX_Users_Email ON Users(Email)"),
                    ("Normalization", "Organizing tables to reduce duplicated data.", "Moving customer details out of the Orders table (3NF)")
                }),
                ("Data Structures Basics", "Stack, queue, hash table, tree and Big-O", "admin_teacher", "DSA101", new[]
                {
                    ("Stack", "Last In, First Out (LIFO) collection.", "Undo history in a text editor"),
                    ("Queue", "First In, First Out (FIFO) collection.", "A printer job queue"),
                    ("Hash Table", "Stores key-value pairs with O(1) average lookup.", "Dictionary<string, int> in C#"),
                    ("Binary Search Tree", "Left child is smaller, right child is larger; O(log n) search when balanced.", "Keeping a sorted set of IDs"),
                    ("Big-O Notation", "Describes how running time grows with input size.", "Binary search is O(log n)")
                }),
                ("Python Basics", "Lists, dictionaries and functions", "admin_teacher", "PY101", new[]
                {
                    ("List", "An ordered, changeable collection.", "nums = [1, 2, 3]"),
                    ("Dictionary", "A collection of key-value pairs.", "user = {'name': 'Alice', 'age': 21}"),
                    ("List Comprehension", "A short way to build a list from another sequence.", "[x * x for x in range(5)]"),
                    ("Function", "A reusable block of code defined with def.", "def greet(name): return 'Hi ' + name")
                })
            };
            foreach (var demo in demoSets)
            {
                string ownerId = users[demo.Owner].Id;
                if (context.StudySets.Any(s => s.Title == demo.Title && s.OwnerUserId == ownerId)) continue;

                var studySet = new StudySet { Title = demo.Title, Description = demo.Description, OwnerUserId = ownerId };
                context.StudySets.Add(studySet);
                context.SaveChanges();

                context.Flashcards.AddRange(demo.Cards.Select(card => new Flashcard
                {
                    StudySetId = studySet.StudySetId,
                    Term = card.Term,
                    Definition = card.Definition,
                    Example = card.Example
                }));

                if (demo.ClassCode != null)
                {
                    var classroom = context.Classrooms.FirstOrDefault(c => c.InviteCode == demo.ClassCode);
                    if (classroom != null)
                    {
                        context.ClassRoomMaterials.Add(new ClassRoomMaterial
                        {
                            ClassRoomId = classroom.ClassroomId,
                            StudySetId = studySet.StudySetId,
                            Status = ClassroomMaterialStatus.Available
                        });
                    }
                }
                context.SaveChanges();
            }

            // 3) Điểm cho bảng xếp hạng: chỉ gán cho tài khoản còn ở mức mặc định (Level 1, 0 XP)
            //    để không ghi đè tiến độ thật. XP < 1000 vì cứ 1000 XP thì lên 1 cấp.
            var demoStats = new (string User, int Level, int Xp, int Streak)[]
            {
                ("sv_it_01", 8, 420, 15),
                ("sv_nn_03", 7, 760, 11),
                ("sv_it_02", 6, 300, 9),
                ("demo_student", 5, 650, 6),
                ("sv_it_04", 4, 150, 3),
                ("demo_teacher", 3, 500, 2)
            };
            foreach (var stat in demoStats)
            {
                var user = users[stat.User];
                if (user.Level == 1 && user.XP == 0)
                {
                    user.Level = stat.Level;
                    user.XP = stat.Xp;
                    user.CurrentStreak = stat.Streak;
                    user.HighestStreak = Math.Max(user.HighestStreak, stat.Streak);
                    user.LastStudyDate = DateTime.Now;
                }
            }
            context.SaveChanges();
        }
    }
}