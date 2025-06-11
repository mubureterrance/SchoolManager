using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.HasKey(d => d.DepartmentId);
            builder.Property(d => d.DepartmentId).HasDefaultValueSql("NEWID()");
            builder.Property(d => d.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(d => d.IsActive).HasDefaultValue(true);

            builder.HasIndex(d => d.Name).IsUnique();
            builder.HasIndex(d => d.HeadOfDepartmentId);

            builder.HasOne(d => d.HeadOfDepartment)
                   .WithMany() // If reverse navigation is not needed
                   .HasForeignKey(d => d.HeadOfDepartmentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.Subjects)
                   .WithOne(s => s.Department)
                   .HasForeignKey(s => s.DepartmentId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
