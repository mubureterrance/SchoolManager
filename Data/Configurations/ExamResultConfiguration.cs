using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations
{
    public class ExamResultConfiguration : IEntityTypeConfiguration<ExamResult>
    {
        public void Configure(EntityTypeBuilder<ExamResult> builder)
        {
            builder.HasKey(e => e.ResultId);
            builder.Property(e => e.ResultId).HasDefaultValueSql("NEWID()");
            builder.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(e => new { e.ExamId, e.StudentId, e.SubjectId }).IsUnique();

            builder.HasOne(e => e.Exam)
                   .WithMany(ex => ex.ExamResults)
                   .HasForeignKey(e => e.ExamId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Student)
                   .WithMany(s => s.ExamResults)
                   .HasForeignKey(e => e.StudentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Subject)
                   .WithMany(s => s.ExamResults)
                   .HasForeignKey(e => e.SubjectId)
                   .OnDelete(DeleteBehavior.Restrict); // Preserves results if subject is deleted
        }
    }
}
