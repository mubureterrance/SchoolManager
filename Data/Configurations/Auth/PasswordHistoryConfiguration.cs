using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations.Auth
{
    public class PasswordHistoryConfiguration : IEntityTypeConfiguration<PasswordHistory>
    {
        public void Configure(EntityTypeBuilder<PasswordHistory> builder)
        {
            builder.HasKey(ph => ph.PasswordHistoryId);
            builder.Property(ph => ph.PasswordHistoryId).HasDefaultValueSql("NEWID()");
            builder.Property(ph => ph.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(ph => ph.UserId);

            builder.HasOne(ph => ph.User)
                   .WithMany(u => u.PasswordHistory)
                   .HasForeignKey(ph => ph.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
