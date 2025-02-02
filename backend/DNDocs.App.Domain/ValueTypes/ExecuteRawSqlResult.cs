namespace DNDocs.Domain.ValueTypes
{
    public class ExecuteRawSqlResult
    {
        public string[] Columns { get; set; }
        public object[][] Rows { get; set; }

        public int ExecuteMode { get; set; }
        public int? ExecuteNonQueryResult { get; set; }
        public bool Success { get; set; }
        public string Exception { get; set; }
    }
}
