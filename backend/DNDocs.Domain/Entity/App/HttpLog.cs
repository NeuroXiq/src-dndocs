namespace DNDocs.Domain.Entity.App
{
    public class HttpLog : Entity
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
