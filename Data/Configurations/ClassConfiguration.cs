using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations
{
    public class ClassConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            builder.HasKey(e => e.ClassId);
            builder.Property(e => e.ClassId).HasDefaultValueSql("NEWID()");
            builder.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(e => e.IsActive).HasDefaultValue(true);

            builder.HasIndex(c => new { c.ClassName, c.Section, c.AcademicYear }).IsUnique();

            builder.HasOne(c => c.ClassTeacher)
                   .WithMany()
                   .HasForeignKey(c => c.ClassTeacherId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(c => c.Students)
                   .WithOne(s => s.Class)
                   .HasForeignKey(s => s.ClassId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(c => c.Attendances)
                   .WithOne(a => a.Class)
                   .HasForeignKey(a => a.ClassId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.FeeStructures)
                   .WithOne(f => f.Class)
                   .HasForeignKey(f => f.ClassId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
