using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations.Auth
{
    public class TwoFactorTokenConfiguration : IEntityTypeConfiguration<TwoFactorToken>
    {
        public void Configure(EntityTypeBuilder<TwoFactorToken> builder)
        {
            builder.HasKey(t => t.TokenId);
            builder.Property(t => t.TokenId).HasDefaultValueSql("NEWID()");
            builder.Property(t => t.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(t => t.IsUsed).HasDefaultValue(false);
            builder.Property(t => t.AttemptCount).HasDefaultValue(0);

            builder.HasIndex(t => t.UserId);

            builder.HasOne(t => t.User)
                   .WithMany()
                   .HasForeignKey(t => t.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
