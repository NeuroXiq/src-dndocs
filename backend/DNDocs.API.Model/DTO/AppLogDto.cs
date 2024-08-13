namespace DNDocs.Api.DTO
{
    public class AppLogDto
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public string? CategoryName { get; set; }
        public int LogLevelId { get; set; }
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public DateTime? Date { get; set; }
    }
}
