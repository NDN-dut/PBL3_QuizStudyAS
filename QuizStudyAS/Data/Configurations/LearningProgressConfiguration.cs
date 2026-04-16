using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class LearningProgressConfiguration : IEntityTypeConfiguration<LearningProgress>
    {
        public void Configure(EntityTypeBuilder<LearningProgress> builder)
        {
            builder.ToTable("LearningProgresses");
            builder.HasKey(lp => lp.ProgressId);

            builder.HasOne(lp => lp.User)
                   .WithMany(u => u.LearningProgresses)
                   .HasForeignKey(lp => lp.UserId)
                   .OnDelete(DeleteBehavior.Restrict); // Tránh lỗi Multiple Cascade

            builder.HasOne(lp => lp.Flashcard)
                   .WithMany() // Flashcard không cần thiết chứa danh sách Progress
                   .HasForeignKey(lp => lp.FlashcardId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}