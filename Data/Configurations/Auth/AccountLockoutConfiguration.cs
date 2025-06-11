using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations.Auth
{
    public class AccountLockoutConfiguration : IEntityTypeConfiguration<AccountLockout>
    {
        public void Configure(EntityTypeBuilder<AccountLockout> builder)
        {
            builder.HasKey(al => al.LockoutId);
            builder.Property(al => al.LockoutId).HasDefaultValueSql("NEWID()");
            builder.Property(al => al.LockoutStartDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(al => al.IsActive).HasDefaultValue(true);

            builder.HasIndex(al => al.UserId);
            builder.HasIndex(al => al.LockoutEndDate);

            builder.HasOne(al => al.User)
                   .WithMany()
                   .HasForeignKey(al => al.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
