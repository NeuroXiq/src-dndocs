using DNDocs.Api.DTO.Enum;

namespace DNDocs.Api.DTO.Admin
{
    public class ExecRawSqlResultDto
    {
        public string[] Columns { get; set; }
        public object[][] Rows { get; set; }

        public RawSqlExecuteMode ExecuteMode { get; set; }
        public int? ExecuteNonQueryResult { get; set; }
        public bool Success { get; set; }
        public string Exception { get; set; }
    }
}
