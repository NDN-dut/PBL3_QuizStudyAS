using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class ClassRoomMaterialConfiguration  : IEntityTypeConfiguration<ClassRoomMaterial> 
    {
        public void Configure(EntityTypeBuilder<ClassRoomMaterial> builder)
        {
            builder.ToTable("ClassRoom_Material");
            builder.HasKey(e => new { e.ClassRoomId, e.StudySetId });

            builder.Property(r => r.Status)
           .IsRequired()
           .HasMaxLength(20);

            builder.HasOne(e => e.ClassRoom)
                .WithMany(p => p.Materials)
                .HasForeignKey(e => e.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.StudySet)
                .WithMany(p => p.MaterialsOf)
                .HasForeignKey(e => e.StudySetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
