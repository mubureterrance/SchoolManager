using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(e => e.StudentId);
            builder.Property(e => e.StudentId).HasDefaultValueSql("NEWID()");
            builder.Property(e => e.IsActive).HasDefaultValue(true);
            builder.HasIndex(e => e.StudentNumber).IsUnique();

            builder.HasIndex(s => s.UserId);
            builder.HasIndex(s => s.ClassId);

            builder.HasMany(s => s.StudentParents)
                   .WithOne(sp => sp.Student)
                   .HasForeignKey(sp => sp.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
