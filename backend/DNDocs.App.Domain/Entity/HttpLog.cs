namespace DNDocs.Domain.Entity
{
    public class HttpLog : EntityBase
    {
        public int Id { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public string Headers { get; set; }
        public string IP { get; set; }
        public DateTime DateTime { get; set; }
        public string Payload { get; set; }
    }
}
