using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class RequestJoinClassConfiguration : IEntityTypeConfiguration<RequestJoinClass>
    {

        public void Configure(EntityTypeBuilder<RequestJoinClass> builder)
        {
            builder.ToTable("Request_Join_Class");
            builder.HasKey(e => new { e.UserId, e.ClassroomId });

            builder.Property(r => r.Status)
           .IsRequired()
           .HasMaxLength(20);

            builder.HasOne(e => e.User)
                .WithMany(p => p.JoinClassRooms)
                .HasForeignKey(e => e.UserId);

            builder.HasOne(e=>e.Classroom)
                .WithMany(p => p.JoinRequests)
                .HasForeignKey(e=>e.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bổ sung đồng bộ Query Filter: Ẩn Yêu cầu nếu lớp học đã bị xóa
            builder.HasQueryFilter(r => r.Classroom.StatusId != (int)ClassroomStatusEnum.DeletedByUser);
        }
    }
}
