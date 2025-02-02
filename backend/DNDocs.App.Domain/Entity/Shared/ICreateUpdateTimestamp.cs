namespace DNDocs.Domain.Entity.Shared
{
    public interface ICreateUpdateTimestamp
    {
        DateTime CreatedOn { get; set; }
        DateTime LastModifiedOn { get; set; }
    }
}
