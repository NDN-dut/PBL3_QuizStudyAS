using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class ClassroomConfiguration : IEntityTypeConfiguration<Classroom>
    {
        public void Configure(EntityTypeBuilder<Classroom> builder)
        {
            builder.ToTable("Classrooms");
            builder.HasKey(c => c.ClassroomId);

            builder.Property(c => c.ClassName).IsRequired().HasMaxLength(200);
            builder.Property(c => c.InviteCode).IsRequired().HasMaxLength(20);

            // Liên kết với Chủ lớp (OwnerUser)
            builder.HasOne(c => c.OwnerUser)
                   .WithMany(u => u.OwnedClassrooms)
                   .HasForeignKey(c => c.OwnerUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}