namespace SchoolManager.Models.Base
{
    public interface IAuditableEntity
    {
        DateTime CreatedDate { get; set; }
        DateTime? LastModifiedDate { get; set; }
    }
}
