using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations
{
    public class ParentConfiguration : IEntityTypeConfiguration<Parent>
    {
        public void Configure(EntityTypeBuilder<Parent> builder)
        {
            builder.HasKey(e => e.ParentId);
            builder.Property(e => e.ParentId).HasDefaultValueSql("NEWID()");
            builder.Property(e => e.IsActive).HasDefaultValue(true);

            builder.HasIndex(p => p.UserId);

            builder.HasOne(p => p.User)
                   .WithOne(u => u.Parent)
                   .HasForeignKey<Parent>(p => p.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.StudentParents)
                   .WithOne(sp => sp.Parent)
                   .HasForeignKey(sp => sp.ParentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
