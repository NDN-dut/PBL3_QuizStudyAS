using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
    {
        public void Configure(EntityTypeBuilder<QuestionOption> builder)
        {
            builder.ToTable("QuestionOptions");
            builder.HasKey(o => o.OptionId);

            builder.Property(o => o.Content)
                   .IsRequired();

            builder.Property(o => o.IsCorrect)
                   .HasDefaultValue(false);

            // Liên kết với ExamQuestion: Xóa câu hỏi thì tự động xóa các lựa chọn đáp án đi kèm
            builder.HasOne(o => o.Question)
                   .WithMany(q => q.Options)
                   .HasForeignKey(o => o.QuestionId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Ẩn đáp án nếu phòng học của kì thi bị xóa mềm
            builder.HasQueryFilter(o => o.Question.Exam.Classroom.StatusId != (int)ClassroomStatusEnum.DeletedByUser);
        }
    }
}