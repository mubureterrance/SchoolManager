using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations.Auth
{
    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder)
        {
            builder.HasKey(us => us.SessionId);
            builder.Property(us => us.SessionId).HasDefaultValueSql("NEWID()");
            builder.Property(us => us.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(us => us.IsActive).HasDefaultValue(true);

            builder.HasIndex(us => us.SessionToken).IsUnique();
            builder.HasIndex(us => us.RefreshToken);
            builder.HasIndex(us => us.UserId);
            builder.HasIndex(us => us.ExpiryDate);

            builder.HasOne(us => us.User)
                   .WithMany(u => u.UserSessions)
                   .HasForeignKey(us => us.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
