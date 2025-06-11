using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations
{
    public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
    {
        public void Configure(EntityTypeBuilder<Subject> builder)
        {
            builder.HasKey(e => e.SubjectId);
            builder.Property(e => e.SubjectId).HasDefaultValueSql("NEWID()");
            builder.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(e => e.IsActive).HasDefaultValue(true);

            builder.HasIndex(s => s.SubjectCode).IsUnique();
            builder.HasIndex(s => s.SubjectName).IsUnique();

            builder.HasOne(s => s.Department)
                   .WithMany(d => d.Subjects)
                   .HasForeignKey(s => s.DepartmentId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
