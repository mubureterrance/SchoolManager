using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations.Identity
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).HasDefaultValueSql("NEWID()");
            builder.Property(u => u.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(u => u.IsActive).HasDefaultValue(true);
            builder.Property(u => u.IsFirstLogin).HasDefaultValue(true);
            builder.Property(u => u.RequirePasswordChange).HasDefaultValue(true);

            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.PhoneNumber);
            builder.HasIndex(u => u.IsActive);
            builder.HasIndex(u => u.CreatedDate);

            builder.HasOne(u => u.Student)
                   .WithOne(s => s.User)
                   .HasForeignKey<Student>(s => s.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.Staff)
                   .WithOne(s => s.User)
                   .HasForeignKey<Staff>(s => s.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.Parent)
                   .WithOne(p => p.User)
                   .HasForeignKey<Parent>(p => p.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.UserRoles)
                   .WithOne(ur => ur.User)
                   .HasForeignKey(ur => ur.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.UserSessions)
                   .WithOne(us => us.User)
                   .HasForeignKey(us => us.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.LoginHistory)
                   .WithOne(lh => lh.User)
                   .HasForeignKey(lh => lh.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.PasswordHistory)
                   .WithOne(ph => ph.User)
                   .HasForeignKey(ph => ph.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(u => u.Gender)
                   .HasConversion<string>();
        }
    }
}
