using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations.Auth
{
    public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.HasKey(t => t.TokenId);
            builder.Property(t => t.TokenId).HasDefaultValueSql("NEWID()");
            builder.Property(t => t.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(t => t.IsUsed).HasDefaultValue(false);

            builder.HasIndex(t => t.Token).IsUnique();
            builder.HasIndex(t => t.UserId);

            builder.HasOne(t => t.User)
                   .WithMany()
                   .HasForeignKey(t => t.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
