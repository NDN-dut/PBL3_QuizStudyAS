using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Thiết lập khóa chính cho User
            builder.HasKey(u => u.UserId);

            // Cấu hình quan hệ 1-n
            builder.HasMany(u => u.Quizzes)      // Một User có nhiều Quizzes
                .WithOne(q => q.User)           // Mỗi Quiz thuộc về một User
                .HasForeignKey(q => q.UserId)   // Khóa ngoại nằm ở bảng Quiz (trường UserId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa User thì tự động xóa các Quiz liên quan
        }
    }
}
