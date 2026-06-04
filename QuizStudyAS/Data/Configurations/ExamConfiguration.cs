using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class ExamConfiguration : IEntityTypeConfiguration<Exam>
    {
        public void Configure(EntityTypeBuilder<Exam> builder)
        {
            builder.ToTable("Exams");
            builder.HasKey(e => e.ExamId);

            builder.Property(e => e.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.Description)
                   .HasMaxLength(1000)
                   .IsRequired(false);

            builder.Property(e => e.StartTime)
                   .IsRequired();

            builder.Property(e => e.EndTime)
                   .IsRequired();

            builder.Property(e => e.DurationMinutes)
                   .IsRequired();

            builder.Property(e => e.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            // Liên kết với Classroom: Nếu xóa Classroom thì xóa toàn bộ kì thi bên trong
            builder.HasOne(e => e.Classroom)
                   .WithMany() // Không giả định Classroom có thuộc tính Exams để tránh sửa file cũ
                   .HasForeignKey(e => e.ClassroomId)
                   .OnDelete(DeleteBehavior.Cascade);

            // BỔ SUNG DÒNG NÀY ĐỂ GIẢI QUYẾT TRIỆT ĐỂ WARNING 10622
            // Ẩn toàn bộ bài kiểm tra nếu phòng học chứa nó đã bị xóa mềm
            builder.HasQueryFilter(e => e.Classroom.StatusId != (int)ClassroomStatusEnum.DeletedByUser);
        }
    }
}