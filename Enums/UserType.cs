using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SchoolManager.Enums
{
    public enum UserType
    {
        [Description("Super Admin")]
        SuperAdmin = 1,

        [Description("System Admin")]
        Admin = 2,

        [Description("School Principal")]
        Principal = 3,

        [Description("Class Teacher")]
        Teacher = 4,

        [Description("Student")]
        Student = 5,

        [Description("Parent/Guardian")]
        Parent = 6,

        [Description("School Staff")]
        Staff = 7
    }

    public enum LoginResult
    {
        [Description("Login Successful")]
        Success = 1,

        [Description("Login Failed")]
        Failed = 2,

        [Description("Account Locked")]
        AccountLocked = 3,

        [Description("Account Disabled")]
        AccountDisabled = 4,

        [Description("Password Expired")]
        PasswordExpired = 5,

        [Description("Two-Factor Authentication Required")]
        TwoFactorRequired = 6,

        [Description("Invalid Credentials")]
        InvalidCredentials = 7
    }

    public enum TokenType
    {
        [Description("SMS Token")]
        SMS=1,

        [Description("Email Token")]
        Email=2,

        [Description("Authenticator Token")]
        Authenticator=3,

        [Description("Backup Code")]
        Backup=4
    }

    public enum LockoutReason
    {
        [Description("Failed Login Attempts")]
        FailedLoginAttempts = 1,

        [Description("Security Violation")]
        SecurityViolation = 2,

        [Description("Administrator Action")]   
        AdminAction = 3,

        [Description("Suspicious Activity Detected")]
        SuspiciousActivity = 4,

        [Description("Policy Violation")]
        PolicyViolation = 5
    }

    public enum Gender
    {
        [Description("Male")]
        Male = 1,

        [Description("Female")]
        Female = 2,

        [Description("Other")]
        Other = 3,

        [Description("Not Spesified")]
        NotSpecifie = 4,
    }

    public enum AttendanceStatus
    {
        [Description("Present")]
        Present = 1,

        [Description("Absent")]
        Absent = 2,

        [Description("Late")]
        Late =3
    }

    public enum ExamStatus
    {
        [Description("Up Coming")]
        Upcoming = 1,

        [Description("On Going")]
        OnGoing = 2,

        [Description("Completed")]
        Completed =3,

        [Description("Cancelled")]
        Cancelled = 4,
    }

    public enum PaymentStatus
    {
        [Description("Pending")]
        Pending = 1,

        [Description("Completed")]
        Completed = 2,

        [Description("Failed")]
        Failed = 3,

        [Description("Reversed")]
        Reversed = 4
    }

    public enum PaymentMethod
    {
        [Description("Cash")]
        Cash = 1,

        [Description("Bank Transfer")]
        BankTransfer = 2,

        [Description("Mobile Money")]
        MobileMoney = 3,

        [Description("Credit Card")]
        CreditCard = 4,

        [Description("Debit Card")]
        DebitCard = 5,

        [Description("Cheque")]
        Cheque = 6,

        [Description("On-line Payment")]
        OnlinePayment = 7,

        [Description("Point of Sale")]
        POS = 8 // Point of Sale

    }

    public enum ParentRelationship
    {
        [Description("Mother")]
        Mother = 1,

        [Description("Father")]
        Father = 2,

        [Description("Guardian")]
        Guardian = 3,

        [Description("Other")]
        Other = 4
    }


    public static class Helpers
    {
        private static readonly ConcurrentDictionary<Enum, string> DisplayNameCache = new();

        public static string GetDisplayName(Enum value)
        {
            if (value == null)
                return string.Empty;

            return DisplayNameCache.GetOrAdd(value, GetDisplayNameInternal);
        }

        private static string GetDisplayNameInternal(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            if (field == null)
                return value.ToString();

            // Check for DescriptionAttribute first (matches your enum pattern)
            var descriptionAttr = field.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttr?.Description != null)
                return descriptionAttr.Description;

            // Check for DisplayAttribute
            var displayAttr = field.GetCustomAttribute<DisplayAttribute>();
            if (displayAttr?.Name != null)
                return displayAttr.Name;

            // Fallback to DisplayNameAttribute
            var displayNameAttr = field.GetCustomAttribute<DisplayNameAttribute>();
            if (displayNameAttr?.DisplayName != null)
                return displayNameAttr.DisplayName;

            // Final fallback to enum string representation
            return value.ToString();
        }

    }
}
