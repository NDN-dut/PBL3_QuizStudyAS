using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class StudySetConfiguration : IEntityTypeConfiguration<StudySet>
    {
        public void Configure(EntityTypeBuilder<StudySet> builder)
        {
            builder.ToTable("StudySets");
            builder.HasKey(s => s.StudySetId);

            builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
            builder.Property(s => s.IsActive).HasDefaultValue(true);

            // Liên kết với User (Người tạo)
            builder.HasOne(s => s.OwnerUser)
                   .WithMany(u => u.StudySets)
                   .HasForeignKey(s => s.OwnerUserId)
                   .OnDelete(DeleteBehavior.Restrict); // Dùng Restrict để tránh lỗi đa luồng xóa

            
        }
    }
}