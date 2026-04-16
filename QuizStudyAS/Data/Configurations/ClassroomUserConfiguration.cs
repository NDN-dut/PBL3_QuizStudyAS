using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class ClassroomUserConfiguration : IEntityTypeConfiguration<ClassroomUser>
    {
        public void Configure(EntityTypeBuilder<ClassroomUser> builder)
        {
            builder.ToTable("ClassroomUsers");

            // Khóa chính cấu thành từ 2 cột
            builder.HasKey(cu => new { cu.ClassroomId, cu.UserId });

            builder.HasOne(cu => cu.Classroom)
                   .WithMany(c => c.ClassroomUsers)
                   .HasForeignKey(cu => cu.ClassroomId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cu => cu.User)
                   .WithMany(u => u.JoinedClassrooms)
                   .HasForeignKey(cu => cu.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}