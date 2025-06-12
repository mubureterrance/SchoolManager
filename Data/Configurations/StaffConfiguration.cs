using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations
{
    public class StaffConfiguration : IEntityTypeConfiguration<Staff>
    {
        public void Configure(EntityTypeBuilder<Staff> builder)
        {
            builder.HasKey(e => e.StaffId);
            builder.Property(e => e.StaffId).HasDefaultValueSql("NEWID()");
            builder.Property(e => e.IsActive).HasDefaultValue(true);
            builder.HasIndex(e => e.EmployeeNumber).IsUnique();

            builder.HasIndex(s => s.UserId);

            builder.HasOne(s => s.User)
                   .WithOne(u => u.Staff)
                   .HasForeignKey<Staff>(s => s.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
