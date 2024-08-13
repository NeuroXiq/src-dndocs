namespace DNDocs.Docs.Web.Model
{
    //public enum DataSectorState
    //{
    //    Active = 1,
    //    Inactive = 2,
    //}

    public class DataSector
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        

        public DateTime? LastBackupDate { get; set; }


        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }

    }
}
