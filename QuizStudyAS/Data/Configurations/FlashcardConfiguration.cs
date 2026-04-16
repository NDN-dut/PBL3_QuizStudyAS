using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class FlashcardConfiguration : IEntityTypeConfiguration<Flashcard>
    {
        public void Configure(EntityTypeBuilder<Flashcard> builder)
        {
            builder.ToTable("Flashcards");
            builder.HasKey(f => f.FlashcardId);

            builder.Property(f => f.Term).IsRequired().HasMaxLength(255);
            builder.Property(f => f.Definition).IsRequired();

            // Liên kết với StudySet
            builder.HasOne(f => f.StudySet)
                   .WithMany(s => s.Flashcards)
                   .HasForeignKey(f => f.StudySetId)
                   .OnDelete(DeleteBehavior.Cascade); // Xóa bộ thẻ -> Xóa sạch Flashcard bên trong
        }
    }
}