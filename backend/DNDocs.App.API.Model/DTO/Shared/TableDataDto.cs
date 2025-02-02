namespace DNDocs.Api.DTO.Shared
{
    public class TableData
    {
        public int? RowsCount { get; set; }
        public int? PagesCount { get { return RowsCount.HasValue ? (int)Math.Ceiling((double)RowsCount.Value / RowsPerPage) : null; } }
        public int CurrentPage { get; set; }
        public int RowsPerPage { get; set; }

        public IEnumerable<object> Result { get; set; }

        public TableData(int? rowsCount, int currentPage, int rowsPerPage, IEnumerable<object> result)
        {
            RowsCount = rowsCount;
            CurrentPage = currentPage;
            RowsPerPage = rowsPerPage;
            Result = result;
        }
    }

    public class TableDataDto<TDto>
    {
        public int? RowsCount { get; set; }
        public int? PagesCount { get { return RowsCount.HasValue ? (int)Math.Ceiling((double)RowsCount.Value / RowsPerPage) : null; } }
        public int CurrentPage { get; set; }
        public int RowsPerPage { get; set; }

        public IEnumerable<object> Data { get; set; }

        public TableDataDto(int? rowsCount, int currentPage, int rowsPerPage, IEnumerable<object> result)
        {
            RowsCount = rowsCount;
            CurrentPage = currentPage;
            RowsPerPage = rowsPerPage;
            Data = result;
        }

    }
}
