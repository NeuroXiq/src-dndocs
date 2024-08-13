namespace DNDocs.Web.Models.Admin
{
    public class AttachTenantModel
    {
        public int TenantId { get; set; }
        public IFormFile File { get; set; }
    }
}
