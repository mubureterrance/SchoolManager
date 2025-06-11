using Microsoft.EntityFrameworkCore;
using SchoolManager.Models;

namespace SchoolManager.Data.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void SeedSystemData(this ModelBuilder builder)
        {
            var adminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var principalRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var teacherRoleId = Guid.Parse("00000000-0000-0000-0000-000000000003");
            var studentRoleId = Guid.Parse("00000000-0000-0000-0000-000000000004");
            var parentRoleId = Guid.Parse("00000000-0000-0000-0000-000000000005");
            var staffRoleId = Guid.Parse("00000000-0000-0000-0000-000000000006");

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

            builder.Entity<Permission>().HasData(
            // Seed system permissions
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
            
            );
        }
    }
}

