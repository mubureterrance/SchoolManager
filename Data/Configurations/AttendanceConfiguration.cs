using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.HasKey(a => a.AttendanceId);
            builder.Property(a => a.AttendanceId).HasDefaultValueSql("NEWID()");
            builder.Property(a => a.AttendanceDate).HasDefaultValueSql("GETUTCDATE()");

            builder.Property(a => a.Status)
                   .HasConversion<string>();

            builder.HasIndex(a => new { a.StudentId, a.ClassId, a.AttendanceDate })
                   .IsUnique();

            builder.HasOne(a => a.Student)
                   .WithMany(s => s.Attendances)
                   .HasForeignKey(a => a.StudentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Class)
                   .WithMany(c => c.Attendances)
                   .HasForeignKey(a => a.ClassId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
