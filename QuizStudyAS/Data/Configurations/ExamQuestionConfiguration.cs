using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class ExamQuestionConfiguration : IEntityTypeConfiguration<ExamQuestion>
    {
        public void Configure(EntityTypeBuilder<ExamQuestion> builder)
        {
            builder.ToTable("ExamQuestions");
            builder.HasKey(q => q.QuestionId);

            builder.Property(q => q.Content)
                   .IsRequired();

            builder.Property(q => q.Explanation)
                   .IsRequired(false);

            // Liên kết với Exam: Xóa đề thi thì xóa sạch câu hỏi của đề đó
            builder.HasOne(q => q.Exam)
                   .WithMany(e => e.ExamQuestions)
                   .HasForeignKey(q => q.ExamId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Ẩn câu hỏi nếu phòng học của kì thi bị xóa mềm
            builder.HasQueryFilter(q => q.Exam.Classroom.StatusId != (int)ClassroomStatusEnum.DeletedByUser);
        }
    }
}