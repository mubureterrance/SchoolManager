using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations.Auth
{
    public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
    {
        public void Configure(EntityTypeBuilder<UserPermission> builder)
        {
            builder.HasKey(up => up.UserPermissionId);
            builder.Property(up => up.UserPermissionId).HasDefaultValueSql("NEWID()");
            builder.Property(up => up.GrantedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(up => up.IsGranted).HasDefaultValue(true);

            builder.HasIndex(up => new { up.UserId, up.PermissionId }).IsUnique();

            builder.HasOne(up => up.User)
                   .WithMany()
                   .HasForeignKey(up => up.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(up => up.Permission)
                   .WithMany(p => p.UserPermissions)
                   .HasForeignKey(up => up.PermissionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
