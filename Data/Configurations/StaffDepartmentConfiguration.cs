using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations
{
    public class StaffDepartmentConfiguration : IEntityTypeConfiguration<StaffDepartment>
    {
        public void Configure(EntityTypeBuilder<StaffDepartment> builder)
        {
            // Table name
            builder.ToTable("StaffDepartments");

            // Primary Key
            builder.HasKey(sd => sd.StaffDepartmentId);

            // Foreign Key Relationships
            builder.HasOne(sd => sd.Staff)
                .WithMany(s => s.StaffDepartments)
                .HasForeignKey(sd => sd.StaffId)
                .OnDelete(DeleteBehavior.Cascade); // If staff is deleted, remove department assignments

            builder.HasOne(sd => sd.Department)
                .WithMany(d => d.StaffDepartments)
                .HasForeignKey(sd => sd.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent department deletion if staff assigned

            // Indexes for Performance
            // Composite index for active staff-department combinations
            builder.HasIndex(sd => new { sd.StaffId, sd.DepartmentId, sd.IsActive })
                .HasDatabaseName("IX_StaffDepartment_Staff_Department_Active");

            // Index for finding active assignments by staff
            builder.HasIndex(sd => new { sd.StaffId, sd.IsActive })
                .HasDatabaseName("IX_StaffDepartment_Staff_Active");

            // Index for finding active assignments by department
            builder.HasIndex(sd => new { sd.DepartmentId, sd.IsActive })
                .HasDatabaseName("IX_StaffDepartment_Department_Active");

            // Index for finding primary assignments
            builder.HasIndex(sd => new { sd.StaffId, sd.IsPrimary, sd.IsActive })
                .HasDatabaseName("IX_StaffDepartment_Staff_Primary_Active");

            builder.Property(sd => sd.StartDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(sd => sd.EndDate)
                .HasColumnType("date");

            builder.Property(sd => sd.Notes)
                .HasMaxLength(200);

            // Default Values
            builder.Property(sd => sd.IsPrimary)
                .HasDefaultValue(false);

            builder.Property(sd => sd.IsActive)
                .HasDefaultValue(true);

            // Audit Properties
            builder.Property(sd => sd.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()"); // SQL Server

            builder.Property(sd => sd.LastModifiedDate)
                .IsRequired(false);
        }
    }
}
