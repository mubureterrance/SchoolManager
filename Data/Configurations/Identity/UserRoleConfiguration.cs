﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Models;

namespace SchoolManager.Data.Configurations.Identity
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");

            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.Property(ur => ur.AssignedDate)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(ur => ur.IsActive)
                   .HasDefaultValue(true);

            builder.HasOne(ur => ur.User)
                   .WithMany(u => u.UserRoles)
                   .HasForeignKey(ur => ur.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.Role)
                   .WithMany(r => r.UserRoles)
                   .HasForeignKey(ur => ur.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
