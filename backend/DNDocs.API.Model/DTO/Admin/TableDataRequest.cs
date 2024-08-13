namespace DNDocs.Api.DTO.Admin
{
    public class TableDataRequest
    {
        public class Filter
        {
            public string Column { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }

            public Filter(string column, string type, string value)
            {
                Column = column;
                Type = type;
                Value = value;
            }
        }

        public enum OrderDir
        {
            Asc = 1,
            Desc = 2
        }

        public class OrderByInfo
        {
            public string Column { get; set; }
            public OrderDir Dir { get; set; }
        }

        public bool GetRowsCount { get; set; }
        public int Page { get; set; }
        public int RowsPerPage { get; set; }
        public string TableName { get; set; }

        public List<Filter> Filters { get; set; }
        public IList<OrderByInfo> OrderBy { get; set; }

        public TableDataRequest() { }

        public TableDataRequest(int pageno, int rowsperpage, bool getrowscount)
        {
            GetRowsCount = getrowscount;
            Page = pageno;
            RowsPerPage = rowsperpage;
            Filters = new List<Filter>();
        }

        public void AddFilter(string key, string value)
        {
        }
    }
}