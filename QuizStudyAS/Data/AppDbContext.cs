using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Đăng ký tất cả các bảng theo thiết kế ERD
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<StudySet> StudySets { get; set; }
        public DbSet<Flashcard> Flashcards { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<ClassroomUser> ClassroomUsers { get; set; }
        public DbSet<LearningProgress> LearningProgresses { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<QuizQuestionResult> QuizQuestionResults { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ClassRoomMaterial> ClassRoomMaterials { get; set; }
        public DbSet<RequestJoinClass> RequestJoinClasses { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<AuthProvider> AuthProviders { get; set; }
        // BỔ SUNG BẢNG MỚI TẠI ĐÂY
        public DbSet<StudySetStatus> StudySetStatuses { get; set; }
        // BỔ SUNG BẢNG MỚI TẠI ĐÂY
        public DbSet<ClassroomStatus> ClassroomStatuses { get; set; }
        // BỔ SUNG CÁC BẢNG CHO TÍNH NĂNG KIỂM TRA
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<ExamAttempt> ExamAttempts { get; set; }
        public DbSet<ExamAttemptDetail> ExamAttemptDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mồi dữ liệu cho StudySetStatus (Giữ lại để không bị lỗi Migration)
            modelBuilder.Entity<StudySetStatus>().HasData(
                new StudySetStatus { StatusId = 1, Name = "Active" },
                new StudySetStatus { StatusId = 2, Name = "DeletedByUser" },
                new StudySetStatus { StatusId = 3, Name = "LockedByAdmin" }
            );

            // Mồi dữ liệu cho ClassroomStatus
            modelBuilder.Entity<ClassroomStatus>().HasData(
                new ClassroomStatus { StatusId = 1, Name = "Active" },
                new ClassroomStatus { StatusId = 2, Name = "DeletedByUser" },
                new ClassroomStatus { StatusId = 3, Name = "LockedByAdmin" }
            );

            // Tự động quét và áp dụng tất cả các file kế thừa IEntityTypeConfiguration trong Project
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}