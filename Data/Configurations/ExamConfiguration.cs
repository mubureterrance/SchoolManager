using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations
{
    public class ExamConfiguration : IEntityTypeConfiguration<Examination>
    {
        public void Configure(EntityTypeBuilder<Examination> builder)
        {
            builder.HasKey(e => e.ExamId);
            builder.Property(e => e.ExamId).HasDefaultValueSql("NEWID()");
            builder.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(e => new { e.ExamName, e.ExamType, e.StartDate, e.ClassId }).IsUnique();

            builder.Property(e => e.Status)
                   .HasConversion<string>();

            builder.HasMany(e => e.ExamResults)
                   .WithOne(er => er.Exam)
                   .HasForeignKey(er => er.ExamId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
