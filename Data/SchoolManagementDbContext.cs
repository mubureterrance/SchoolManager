using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SchoolManager.Data.Extensions;
using SchoolManager.Models;
using SchoolManager.Models.Base;
using System.Reflection.Emit;

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
        public DbSet<Department> Departments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Examination> Examinations { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<FeeStructure> FeeStructures { get; set; }
        public DbSet<FeePayment> FeePayments { get; set; }

        // ===============================
        // MODEL CONFIGURATION
        // ===============================

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Automatically apply all IEntityTypeConfiguration<T> from this assembly
            builder.ApplyConfigurationsFromAssembly(typeof(SchoolManagementDbContext).Assembly);

            // Seed initial data
            builder.SeedSystemData();
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
                .Where(e => e.Entity is IAuditableEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IAuditableEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedDate = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.LastModifiedDate = DateTime.UtcNow;
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
