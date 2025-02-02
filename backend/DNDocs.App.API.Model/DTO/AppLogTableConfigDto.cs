namespace DNDocs.Api.DTO
{
    public class AppLogTableConfigDto
    {
        public string[] ColumnNames { get; set; }
        public string[] CategoryNames { get; set; }
        public string[] LogLevels { get; set; }

        public AppLogTableConfigDto(
            string[] columnNames,
            string[] categoryNames,
            string[] logLevels)
        {
            ColumnNames = columnNames;
            CategoryNames = categoryNames;
            LogLevels = logLevels;
        }
    }
}
