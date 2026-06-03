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
            // builder.Property(c => c.IsActive).HasDefaultValue(true);

            // THIẾT LẬP MỚI: Trạng thái mặc định là Active
            builder.Property(c => c.StatusId).HasDefaultValue(1);

            // Cấu hình khóa ngoại
            builder.HasOne(c => c.Status)
                   .WithMany(s => s.Classrooms)
                   .HasForeignKey(c => c.StatusId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Bộ lọc ngầm: Chỉ ẩn những lớp đã bị xóa mềm (Id = 2)
            builder.HasQueryFilter(c => c.StatusId != (int)ClassroomStatusEnum.DeletedByUser);

            // Liên kết với Chủ lớp (OwnerUser)
            builder.HasOne(c => c.OwnerUser)
                   .WithMany(u => u.OwnedClassrooms)
                   .HasForeignKey(c => c.OwnerUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}