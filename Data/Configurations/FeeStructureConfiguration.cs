using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations
{
    public class FeeStructureConfiguration : IEntityTypeConfiguration<FeeStructure>
    {
        public void Configure(EntityTypeBuilder<FeeStructure> builder)
        {
            builder.HasKey(f => f.FeeStructureId);
            builder.Property(f => f.FeeStructureId).HasDefaultValueSql("NEWID()");
            builder.Property(f => f.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(f => f.IsActive).HasDefaultValue(true);

            builder.HasIndex(f => new { f.ClassId, f.FeeType, f.AcademicYear }).IsUnique();

            builder.HasOne(f => f.Class)
                   .WithMany(c => c.FeeStructures)
                   .HasForeignKey(f => f.ClassId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
