using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizStudyAS.Models;

namespace QuizStudyAS.Data.Configurations
{
    public class ClassroomPostConfiguration : IEntityTypeConfiguration<ClassroomPost>
    {
        public void Configure(EntityTypeBuilder<ClassroomPost> builder)
        {
            builder.HasKey(p => p.PostId);

            builder.Property(p => p.Content)
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.Property(p => p.CreatedAt)
                   .IsRequired();

            // FK: Post → Classroom (cascade delete: deleting a classroom removes all its posts)
            builder.HasOne(p => p.Classroom)
                   .WithMany(c => c.Posts)
                   .HasForeignKey(p => p.ClassroomId)
                   .OnDelete(DeleteBehavior.Cascade);

            // FK: Post → Author (no cascade: deleting a user won't delete posts)
            builder.HasOne(p => p.Author)
                   .WithMany()
                   .HasForeignKey(p => p.AuthorUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
