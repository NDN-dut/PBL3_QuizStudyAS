using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
    {
        public void Configure(EntityTypeBuilder<GameSession> builder)
        {
            builder.ToTable("GameSessions");
            builder.HasKey(g => g.SessionId);

            // Liên kết với User: Khi xóa User, KHÔNG xóa tự động Session (để tránh lỗi Multiple Cascade)
            builder.HasOne(g => g.User)
                   .WithMany(u => u.GameSessions)
                   .HasForeignKey(g => g.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Liên kết với StudySet: Khi xóa Bộ thẻ, xóa luôn các phiên chơi liên quan
            builder.HasOne(g => g.StudySet)
                   .WithMany() // StudySet có thể không cần danh sách GameSession
                   .HasForeignKey(g => g.StudySetId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}