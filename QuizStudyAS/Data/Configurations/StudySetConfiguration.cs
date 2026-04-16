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

            // Liên kết với User (Người tạo)
            builder.HasOne(s => s.OwnerUser)
                   .WithMany(u => u.StudySets)
                   .HasForeignKey(s => s.OwnerUserId)
                   .OnDelete(DeleteBehavior.Restrict); // Dùng Restrict để tránh lỗi đa luồng xóa

            // Liên kết với Classroom (Tùy chọn)
            builder.HasOne(s => s.Classroom)
                   .WithMany(c => c.StudySets)
                   .HasForeignKey(s => s.ClassroomId)
                   .OnDelete(DeleteBehavior.SetNull); // Nếu lớp bị xóa, bộ thẻ văng ra ngoài thành thẻ cá nhân
        }
    }
}