using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor này nhận cấu hình từ Program.cs truyền vào
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cách tốt nhất: Tự động quét và áp dụng TẤT CẢ các file kế thừa IEntityTypeConfiguration (như UserConfiguration, QuizConfigurations)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
