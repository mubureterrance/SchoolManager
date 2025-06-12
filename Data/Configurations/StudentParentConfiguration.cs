using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;
using SchoolManager.Enums;

namespace SchoolManager.Data.Configurations
{
    public class StudentParentConfiguration : IEntityTypeConfiguration<StudentParent>
    {
        public void Configure(EntityTypeBuilder<StudentParent> builder)
        {
            // Table name (optional if your table naming strategy differs)
            builder.ToTable("StudentParents");

            // Primary Key
            builder.HasKey(sp => sp.StudentParentId);

            // Foreign Key Relationships
            builder.HasOne(sp => sp.Student)
                .WithMany(s => s.StudentParents)
                .HasForeignKey(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // Optional: prevent orphaned links

            builder.HasOne(sp => sp.Parent)
                .WithMany(p => p.StudentParents)
                .HasForeignKey(sp => sp.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique Composite Index
            builder.HasIndex(sp => new { sp.StudentId, sp.ParentId })
                .IsUnique();

            // Enum stored as string
            builder.Property(sp => sp.RelationshipToStudent)
                .HasConversion<string>() // store enum as string
                .HasMaxLength(20)        // match DB field size
                .IsRequired();

            // Boolean flags - can add defaults explicitly if you want
            builder.Property(sp => sp.IsPrimaryContact).HasDefaultValue(false);
            builder.Property(sp => sp.IsEmergencyContact).HasDefaultValue(false);
            builder.Property(sp => sp.CanPickupStudent).HasDefaultValue(true);

            // Audit Dates (Handled via IAuditableEntity elsewhere, but safe to set type)
            builder.Property(sp => sp.CreatedDate).IsRequired();
            builder.Property(sp => sp.LastModifiedDate).IsRequired(false);
        }
    }
}
