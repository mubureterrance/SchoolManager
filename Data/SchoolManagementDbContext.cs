using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SchoolManager.Models;

namespace SchoolManager.Data
{
    // ===============================
    // DATABASE CONTEXT
    // ===============================

    /// <summary>
    /// Main database context for the School Management System
    /// </summary>
    public class SchoolManagementDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
        IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public SchoolManagementDbContext(DbContextOptions<SchoolManagementDbContext> options) : base(options)
        {
        }

        // ===============================
        // DBSETS
        // ===============================

        // Authentication & Authorization
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<UserLoginHistory> UserLoginHistory { get; set; }
        public DbSet<PasswordHistory> PasswordHistory { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<TwoFactorToken> TwoFactorTokens { get; set; }
        public DbSet<AccountLockout> AccountLockouts { get; set; }

        // School-specific entities
        public DbSet<Student> Students { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<StudentParent> StudentParents { get; set; }
        public DbSet<Class> Classes { get; set; }

        // ===============================
        // MODEL CONFIGURATION
        // ===============================

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Identity tables with custom names
            ConfigureIdentityTables(builder);

            // Configure custom entities
            ConfigureUserEntities(builder);
            ConfigureAuthenticationEntities(builder);
            ConfigureSchoolEntities(builder);

            // Configure relationships
            ConfigureRelationships(builder);

            // Configure indexes
            ConfigureIndexes(builder);

            // Seed initial data
            SeedInitialData(builder);
        }

        private void ConfigureIdentityTables(ModelBuilder builder)
        {
            // Rename Identity tables to match our naming convention
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<UserRole>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        }

        private void ConfigureUserEntities(ModelBuilder builder)
        {
            // ApplicationUser configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsFirstLogin).HasDefaultValue(true);
                entity.Property(e => e.RequirePasswordChange).HasDefaultValue(true);

                // Configure computed column for FullName
                //entity.Property(e => e.FullName).HasComputedColumnSql("TRIM([FirstName] + ' ' + ISNULL([MiddleName], '') + ' ' + [LastName])");
            });

            // ApplicationRole configuration
            builder.Entity<ApplicationRole>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsSystemRole).HasDefaultValue(false);
            });

            // UserRole configuration
            builder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
                entity.Property(e => e.AssignedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });
        }

        private void ConfigureAuthenticationEntities(ModelBuilder builder)
        {
            // Permission configuration
            builder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.PermissionId);
                entity.Property(e => e.PermissionId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.PermissionName).IsUnique();
            });

            // RolePermission configuration
            builder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.RolePermissionId);
                entity.Property(e => e.RolePermissionId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.GrantedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
            });

            // UserPermission configuration
            builder.Entity<UserPermission>(entity =>
            {
                entity.HasKey(e => e.UserPermissionId);
                entity.Property(e => e.UserPermissionId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.GrantedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsGranted).HasDefaultValue(true);
                entity.HasIndex(e => new { e.UserId, e.PermissionId }).IsUnique();
            });

            // UserSession configuration
            builder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.SessionId);
                entity.Property(e => e.SessionId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.SessionToken).IsUnique();
                entity.HasIndex(e => e.RefreshToken);
            });

            // UserLoginHistory configuration
            builder.Entity<UserLoginHistory>(entity =>
            {
                entity.HasKey(e => e.LoginHistoryId);
                entity.Property(e => e.LoginHistoryId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.LoginDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // PasswordHistory configuration
            builder.Entity<PasswordHistory>(entity =>
            {
                entity.HasKey(e => e.PasswordHistoryId);
                entity.Property(e => e.PasswordHistoryId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // PasswordResetToken configuration
            builder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(e => e.TokenId);
                entity.Property(e => e.TokenId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsUsed).HasDefaultValue(false);
                entity.HasIndex(e => e.Token).IsUnique();
            });

            // TwoFactorToken configuration
            builder.Entity<TwoFactorToken>(entity =>
            {
                entity.HasKey(e => e.TokenId);
                entity.Property(e => e.TokenId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsUsed).HasDefaultValue(false);
                entity.Property(e => e.AttemptCount).HasDefaultValue(0);
            });

            // AccountLockout configuration
            builder.Entity<AccountLockout>(entity =>
            {
                entity.HasKey(e => e.LockoutId);
                entity.Property(e => e.LockoutId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.LockoutStartDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });
        }

        private void ConfigureSchoolEntities(ModelBuilder builder)
        {
            // Student configuration
            builder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.StudentId);
                entity.Property(e => e.StudentId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.StudentNumber).IsUnique();
            });

            // Staff configuration
            builder.Entity<Staff>(entity =>
            {
                entity.HasKey(e => e.StaffId);
                entity.Property(e => e.StaffId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.EmployeeNumber).IsUnique();
            });

            // Parent configuration
            builder.Entity<Parent>(entity =>
            {
                entity.HasKey(e => e.ParentId);
                entity.Property(e => e.ParentId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // StudentParent configuration
            builder.Entity<StudentParent>(entity =>
            {
                entity.HasKey(e => e.StudentParentId);
                entity.Property(e => e.StudentParentId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsPrimaryContact).HasDefaultValue(false);
                entity.Property(e => e.IsEmergencyContact).HasDefaultValue(false);
                entity.Property(e => e.CanPickupStudent).HasDefaultValue(true);
                entity.HasIndex(e => new { e.StudentId, e.ParentId, e.Relationship }).IsUnique();
            });

            // Class configuration
            builder.Entity<Class>(entity =>
            {
                entity.HasKey(e => e.ClassId);
                entity.Property(e => e.ClassId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => new { e.ClassName, e.Section, e.AcademicYear }).IsUnique();
            });
        }

        private void ConfigureRelationships(ModelBuilder builder)
        {
            // ApplicationUser relationships
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.UserSessions)
                .WithOne(us => us.User)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.LoginHistory)
                .WithOne(lh => lh.User)
                .HasForeignKey(lh => lh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.PasswordHistory)
                .WithOne(ph => ph.User)
                .HasForeignKey(ph => ph.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Student)
                .WithOne(s => s.User)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Staff)
                .WithOne(s => s.User)
                .HasForeignKey<Staff>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Parent)
                .WithOne(p => p.User)
                .HasForeignKey<Parent>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ApplicationRole relationships
            builder.Entity<ApplicationRole>()
                .HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationRole>()
                .HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Permission relationships
            builder.Entity<Permission>()
                .HasMany(p => p.RolePermissions)
                .WithOne(rp => rp.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Permission>()
                .HasMany(p => p.UserPermissions)
                .WithOne(up => up.Permission)
                .HasForeignKey(up => up.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserPermission relationships
            builder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany()
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // PasswordResetToken relationships
            builder.Entity<PasswordResetToken>()
                .HasOne(prt => prt.User)
                .WithMany()
                .HasForeignKey(prt => prt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // TwoFactorToken relationships
            builder.Entity<TwoFactorToken>()
                .HasOne(tft => tft.User)
                .WithMany()
                .HasForeignKey(tft => tft.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AccountLockout relationships
            builder.Entity<AccountLockout>()
                .HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student relationships
            builder.Entity<Student>()
                .HasOne(s => s.CurrentClass)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.CurrentClassId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Student>()
                .HasMany(s => s.StudentParents)
                .WithOne(sp => sp.Student)
                .HasForeignKey(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Parent relationships
            builder.Entity<Parent>()
                .HasMany(p => p.StudentParents)
                .WithOne(sp => sp.Parent)
                .HasForeignKey(sp => sp.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Class relationships
            builder.Entity<Class>()
                .HasOne(c => c.ClassTeacher)
                .WithMany()
                .HasForeignKey(c => c.ClassTeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            // Change from CASCADE to NO ACTION for StudentParents relationships
            builder.Entity<StudentParent>()
                .HasOne(sp => sp.Student)
                .WithMany(s => s.StudentParents)
                .HasForeignKey(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.NoAction); // Changed from Cascade

            builder.Entity<StudentParent>()
                .HasOne(sp => sp.Parent)
                .WithMany(p => p.StudentParents)
                .HasForeignKey(sp => sp.ParentId)
                .OnDelete(DeleteBehavior.NoAction); // Changed from Cascade
        }

        private void ConfigureIndexes(ModelBuilder builder)
        {
            // Performance indexes for commonly queried fields
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.PhoneNumber);

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.IsActive);

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.CreatedDate);

            builder.Entity<UserSession>()
                .HasIndex(us => us.UserId);

            builder.Entity<UserSession>()
                .HasIndex(us => us.ExpiryDate);

            builder.Entity<UserLoginHistory>()
                .HasIndex(ulh => ulh.UserId);

            builder.Entity<UserLoginHistory>()
                .HasIndex(ulh => ulh.LoginDate);

            builder.Entity<UserLoginHistory>()
                .HasIndex(ulh => ulh.IpAddress);

            builder.Entity<Student>()
                .HasIndex(s => s.UserId);

            builder.Entity<Student>()
                .HasIndex(s => s.CurrentClassId);

            builder.Entity<Staff>()
                .HasIndex(s => s.UserId);

            builder.Entity<Staff>()
                .HasIndex(s => s.Department);

            builder.Entity<Parent>()
                .HasIndex(p => p.UserId);

            builder.Entity<StudentParent>()
                .HasIndex(sp => sp.StudentId);

            builder.Entity<StudentParent>()
                .HasIndex(sp => sp.ParentId);
        }

        private void SeedInitialData(ModelBuilder builder)
        {
            // Use fixed GUIDs for system roles
            var adminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var principalRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var teacherRoleId = Guid.Parse("00000000-0000-0000-0000-000000000003");
            var studentRoleId = Guid.Parse("00000000-0000-0000-0000-000000000004");
            var parentRoleId = Guid.Parse("00000000-0000-0000-0000-000000000005");
            var staffRoleId = Guid.Parse("00000000-0000-0000-0000-000000000006");

            // Use fixed date for created dates
            var fixedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = adminRoleId,
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                    Description = "System administrator with full access",
                    IsSystemRole = true,
                    CreatedDate = fixedDate,
                    CreatedBy = "System"
                },
                new ApplicationRole
                {
                    Id = principalRoleId,
                    Name = "Principal",
                    NormalizedName = "PRINCIPAL",
                    Description = "School principal with administrative access",
                    IsSystemRole = true,
                    CreatedDate = fixedDate,
                    CreatedBy = "System"
                },
                new ApplicationRole
                {
                    Id = teacherRoleId,
                    Name = "Teacher",
                    NormalizedName = "TEACHER",
                    Description = "Teaching staff member",
                    IsSystemRole = true,
                    CreatedDate = fixedDate,
                    CreatedBy = "System"
                },
                new ApplicationRole
                {
                    Id = studentRoleId,
                    Name = "Student",
                    NormalizedName = "STUDENT",
                    Description = "Student user",
                    IsSystemRole = true,
                    CreatedDate = fixedDate,
                    CreatedBy = "System"
                },
                new ApplicationRole
                {
                    Id = parentRoleId,
                    Name = "Parent",
                    NormalizedName = "PARENT",
                    Description = "Parent or Guardian",
                    IsSystemRole = true,
                    CreatedDate = fixedDate,
                    CreatedBy = "System"
                },
                new ApplicationRole
                {
                    Id = staffRoleId,
                    Name = "Staff",
                    NormalizedName = "STAFF",
                    Description = "Non-teaching staff member",
                    IsSystemRole = true,
                    CreatedDate = fixedDate,
                    CreatedBy = "System"
                }
            );

            // Seed system permissions
            var permissions = new[]
            {
                // User Management
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000001"), PermissionName = "Users.View", Description = "View users", Category = "User Management", CreatedDate = fixedDate },
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000002"), PermissionName = "Users.Create", Description = "Create users", Category = "User Management", CreatedDate = fixedDate  },
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000003"), PermissionName = "Users.Edit", Description = "Edit users", Category = "User Management", CreatedDate = fixedDate  },
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000004"), PermissionName = "Users.Delete", Description = "Delete users", Category = "User Management", CreatedDate = fixedDate  },
                
                // Student Management
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000005"), PermissionName = "Students.View", Description = "View students", Category = "Student Management" , CreatedDate = fixedDate},
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000006"), PermissionName = "Students.Create", Description = "Create student records", Category = "Student Management" , CreatedDate = fixedDate},
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000007"), PermissionName = "Students.Edit", Description = "Edit student records", Category = "Student Management", CreatedDate = fixedDate },
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000008"), PermissionName = "Students.Delete", Description = "Delete student records", Category = "Student Management", CreatedDate = fixedDate },
                
                // Staff Management
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000009"), PermissionName = "Staff.View", Description = "View staff", Category = "Staff Management", CreatedDate = fixedDate },
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000010"), PermissionName = "Staff.Create", Description = "Create staff records", Category = "Staff Management", CreatedDate = fixedDate },
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000011"), PermissionName = "Staff.Edit", Description = "Edit staff records", Category = "Staff Management", CreatedDate = fixedDate },
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000012"), PermissionName = "Staff.Delete", Description = "Delete staff records", Category = "Staff Management", CreatedDate = fixedDate },
                
                // Academic Management
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000013"), PermissionName = "Classes.View", Description = "View classes", Category = "Academic Management" , CreatedDate = fixedDate},
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000014"), PermissionName = "Classes.Create", Description = "Create classes", Category = "Academic Management", CreatedDate = fixedDate },
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000015"), PermissionName = "Classes.Edit", Description = "Edit classes", Category = "Academic Management", CreatedDate = fixedDate },
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000016"), PermissionName = "Classes.Delete", Description = "Delete classes", Category = "Academic Management", CreatedDate = fixedDate },
                
                // Attendance Management
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000017"), PermissionName = "Attendance.View", Description = "View attendance", Category = "Attendance Management", CreatedDate = fixedDate },
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000018"), PermissionName = "Attendance.Create", Description = "Mark attendance", Category = "Attendance Management" , CreatedDate = fixedDate},
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000019"), PermissionName = "Attendance.Edit", Description = "Edit attendance", Category = "Attendance Management", CreatedDate = fixedDate },
                
                // Grade Management
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000020"), PermissionName = "Grades.View", Description = "View grades", Category = "Grade Management" , CreatedDate = fixedDate},
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000021"), PermissionName = "Grades.Create", Description = "Create grades", Category = "Grade Management" , CreatedDate = fixedDate},
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000022"), PermissionName = "Grades.Edit", Description = "Edit grades", Category = "Grade Management" , CreatedDate = fixedDate},
                
                // Fee Management
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000023"), PermissionName = "Fees.View", Description = "View fee information", Category = "Fee Management" , CreatedDate = fixedDate},
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000024"), PermissionName = "Fees.Create", Description = "Create fee records", Category = "Fee Management" , CreatedDate = fixedDate},
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000025"), PermissionName = "Fees.Edit", Description = "Edit fee records", Category = "Fee Management" , CreatedDate = fixedDate},
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000026"), PermissionName = "Fees.Process", Description = "Process fee payments", Category = "Fee Management" , CreatedDate = fixedDate},
                
                // Reports
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000027"), PermissionName = "Reports.View", Description = "View reports", Category = "Reporting" , CreatedDate = fixedDate},
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000028"), PermissionName = "Reports.Generate", Description = "Generate reports", Category = "Reporting" , CreatedDate = fixedDate},
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000029"), PermissionName = "Reports.Export", Description = "Export reports", Category = "Reporting" , CreatedDate = fixedDate},
                
                // System Administration
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000030"), PermissionName = "System.Configure", Description = "Configure system settings", Category = "System Administration" , CreatedDate = fixedDate},
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000031"), PermissionName = "System.Backup", Description = "Perform system backup", Category = "System Administration" , CreatedDate = fixedDate},
                new Permission { PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000032"), PermissionName = "System.Audit", Description = "View audit logs", Category = "System Administration" , CreatedDate = fixedDate}
            };

            builder.Entity<Permission>().HasData(permissions);
        }

        // ===============================
        // OVERRIDE SAVE CHANGES
        // ===============================

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Automatically set audit fields
            SetAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            // Automatically set audit fields
            SetAuditFields();
            return base.SaveChanges();
        }

        private void SetAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is ApplicationUser &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var user = (ApplicationUser)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    user.CreatedDate = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    user.LastModifiedDate = DateTime.UtcNow;
                }
            }
        }
    }

    // ===============================
    // EXTENSION METHODS
    // ===============================

    public static class DbContextExtensions
    {
        /// <summary>
        /// Get active sessions for a user
        /// </summary>
        public static IQueryable<UserSession> GetActiveSessions(this SchoolManagementDbContext context, Guid userId)
        {
            return context.UserSessions
                .Where(s => s.UserId == userId &&
                           s.IsActive &&
                           s.ExpiryDate > DateTime.UtcNow);
        }

        /// <summary>
        /// Get user permissions (both role-based and direct)
        /// </summary>
        public static async Task<List<string>> GetUserPermissionsAsync(this SchoolManagementDbContext context, Guid userId)
        {
            // Get role-based permissions
            var rolePermissions = await context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Join(context.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.PermissionId)
                .Join(context.Permissions, rp => rp, p => p.PermissionId, (rp, p) => p.PermissionName)
                .ToListAsync();

            // Get direct user permissions (granted)
            var userPermissions = await context.UserPermissions
                .Where(up => up.UserId == userId && up.IsGranted &&
                            (up.ExpiryDate == null || up.ExpiryDate > DateTime.UtcNow))
                .Join(context.Permissions, up => up.PermissionId, p => p.PermissionId, (up, p) => p.PermissionName)
                .ToListAsync();

            // Get direct user permissions (denied)
            var deniedPermissions = await context.UserPermissions
                .Where(up => up.UserId == userId && !up.IsGranted &&
                            (up.ExpiryDate == null || up.ExpiryDate > DateTime.UtcNow))
                .Join(context.Permissions, up => up.PermissionId, p => p.PermissionId, (up, p) => p.PermissionName)
                .ToListAsync();

            // Combine and filter permissions
            var allPermissions = rolePermissions.Union(userPermissions).Except(deniedPermissions).ToList();
            return allPermissions;
        }

        /// <summary>
        /// Check if user has specific permission
        /// </summary>
        public static async Task<bool> HasPermissionAsync(this SchoolManagementDbContext context, Guid userId, string permissionName)
        {
            var permissions = await context.GetUserPermissionsAsync(userId);
            return permissions.Contains(permissionName);
        }

        /// <summary>
        /// Get user's active lockout
        /// </summary>
        public static async Task<AccountLockout?> GetActiveLockoutAsync(this SchoolManagementDbContext context, Guid userId)
        {
            return await context.AccountLockouts
                .Where(al => al.UserId == userId &&
                            al.IsActive &&
                            (al.LockoutEndDate == null || al.LockoutEndDate > DateTime.UtcNow))
                .OrderByDescending(al => al.LockoutStartDate)
                .FirstOrDefaultAsync();
        }
    }
}
