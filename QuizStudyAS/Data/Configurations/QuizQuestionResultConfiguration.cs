using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class QuizQuestionResultConfiguration : IEntityTypeConfiguration<QuizQuestionResult>
    {
        public void Configure(EntityTypeBuilder<QuizQuestionResult> builder)
        {
            builder.ToTable("QuizQuestionResults");
            builder.HasKey(q => q.ResultId);

            builder.HasOne(q => q.GameSession)
                   .WithMany(g => g.QuizQuestionResults)
                   .HasForeignKey(q => q.SessionId)
                   .OnDelete(DeleteBehavior.Cascade); // Xóa Session -> Xóa luôn chi tiết

            builder.HasOne(q => q.Flashcard)
                   .WithMany()
                   .HasForeignKey(q => q.FlashcardId)
                   .OnDelete(DeleteBehavior.Restrict); // Nếu Flashcard bị xóa, kết quả cũ vẫn không làm sập Database
        }
    }
}