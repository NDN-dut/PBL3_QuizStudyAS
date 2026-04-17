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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tự động quét và áp dụng tất cả các file kế thừa IEntityTypeConfiguration trong Project
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}