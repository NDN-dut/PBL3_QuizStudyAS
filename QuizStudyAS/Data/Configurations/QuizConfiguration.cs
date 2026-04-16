using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
    {
        public void Configure(EntityTypeBuilder<Quiz> builder)
        {
            // Định nghĩa khóa phức hợp (Composite Key) bằng đối tượng ẩn danh
            builder.HasKey(q => new { q.UserId, q.Title });

            // Cấu hình các thuộc tính khác (nếu cần)
            builder.Property(q => q.Title)
                .HasMaxLength(200);
        }
    }
}
