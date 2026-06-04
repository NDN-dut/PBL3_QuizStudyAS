using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class ExamAttemptDetailConfiguration : IEntityTypeConfiguration<ExamAttemptDetail>
    {
        public void Configure(EntityTypeBuilder<ExamAttemptDetail> builder)
        {
            builder.ToTable("ExamAttemptDetails");
            builder.HasKey(d => d.AttemptDetailId);

            // Liên kết với ExamAttempt: Xóa lượt làm bài tổng thì xóa chi tiết từng câu trả lời bên trong
            builder.HasOne(d => d.Attempt)
                   .WithMany(a => a.Details)
                   .HasForeignKey(d => d.AttemptId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Liên kết với ExamQuestion: Dùng Restrict để tránh tạo ra vòng lặp xóa từ bảng Exam
            builder.HasOne(d => d.Question)
                   .WithMany()
                   .HasForeignKey(d => d.QuestionId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Liên kết với QuestionOption: Đáp án được chọn có thể null nếu học sinh bỏ trống
            builder.HasOne(d => d.SelectedOption)
                   .WithMany()
                   .HasForeignKey(d => d.SelectedOptionId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Ẩn chi tiết bài làm nếu phòng học bị xóa mềm
            builder.HasQueryFilter(d => d.Attempt.Exam.Classroom.StatusId != (int)ClassroomStatusEnum.DeletedByUser);
        }
    }
}