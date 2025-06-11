using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations.Auth
{
    public class UserLoginHistoryConfiguration : IEntityTypeConfiguration<UserLoginHistory>
    {
        public void Configure(EntityTypeBuilder<UserLoginHistory> builder)
        {
            builder.HasKey(lh => lh.LoginHistoryId);
            builder.Property(lh => lh.LoginHistoryId).HasDefaultValueSql("NEWID()");
            builder.Property(lh => lh.LoginDate).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(lh => lh.UserId);
            builder.HasIndex(lh => lh.LoginDate);
            builder.HasIndex(lh => lh.IpAddress);

            builder.HasOne(lh => lh.User)
                   .WithMany(u => u.LoginHistory)
                   .HasForeignKey(lh => lh.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

