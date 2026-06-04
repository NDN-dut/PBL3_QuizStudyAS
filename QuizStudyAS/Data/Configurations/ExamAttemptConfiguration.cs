using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class ExamAttemptConfiguration : IEntityTypeConfiguration<ExamAttempt>
    {
        public void Configure(EntityTypeBuilder<ExamAttempt> builder)
        {
            builder.ToTable("ExamAttempts");
            builder.HasKey(a => a.AttemptId);

            builder.Property(a => a.StartedAt)
                   .IsRequired();

            builder.Property(a => a.CompletedAt)
                   .IsRequired(false);

            builder.Property(a => a.Score)
                   .HasDefaultValue(0.0);

            builder.Property(a => a.IsSubmitted)
                   .HasDefaultValue(false);

            builder.Property(a => a.IsLate)
                .HasDefaultValue(false); // BỔ SUNG DÒNG NÀY

            // Liên kết với Exam: Xóa kì thi thì xóa lịch sử làm bài của học sinh
            builder.HasOne(a => a.Exam)
                   .WithMany(e => e.Attempts)
                   .HasForeignKey(a => a.ExamId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Liên kết với Users: Dùng Restrict để tránh xung đột đa luồng xóa (Multiple Cascade Paths)
            builder.HasOne(a => a.User)
                   .WithMany()
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Ẩn lịch sử làm bài nếu phòng học bị xóa mềm
            builder.HasQueryFilter(a => a.Exam.Classroom.StatusId != (int)ClassroomStatusEnum.DeletedByUser);
        }
    }
}