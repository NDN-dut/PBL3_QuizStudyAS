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

            //XÓA: builder.Property(s => s.IsActive).HasDefaultValue(true);

            // THIẾT LẬP MỚI: Mặc định khi tạo mới là Active (Id = 1)
            builder.Property(s => s.StatusId).HasDefaultValue(1);
            
            // Cấu hình khóa ngoại cho bảng trạng thái
            builder.HasOne(s => s.Status)
                   .WithMany(st => st.StudySets)
                   .HasForeignKey(s => s.StatusId)
                   .OnDelete(DeleteBehavior.Restrict);

            // THIẾT LẬP MỚI: Chỉ ẩn những học phần đã bị người dùng xóa (Id = 2). 
            // Các trạng thái Active (1) và LockedByAdmin (3) vẫn được truy vấn bình thường.
            builder.HasQueryFilter(s => s.StatusId != (int)StudySetStatusEnum.DeletedByUser);

            // Liên kết với User (Người tạo)
            builder.HasOne(s => s.OwnerUser)
                   .WithMany(u => u.StudySets)
                   .HasForeignKey(s => s.OwnerUserId)
                   .OnDelete(DeleteBehavior.Restrict); // Dùng Restrict để tránh lỗi đa luồng xóa
        }
    }
}