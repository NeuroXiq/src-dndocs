namespace DNDocs.Domain.ValueTypes
{
    //public class TableData
    //{
    //    public int? RowsCount { get; set; }
    //    public int CurrentPage { get; set; }
    //    public int RowsPerPage { get; set; }

    //    public IEnumerable<object> Data { get; set; }

    //    public TableData(int? rowsCount, int currentPage, int rowsPerPage, IEnumerable<object> result)
    //    {
    //        RowsCount = rowsCount;
    //        CurrentPage = currentPage;
    //        RowsPerPage = rowsPerPage;
    //        Result = result;
    //    }
    //}

    public class TableDataResponse
    {
        public int? RowsCount { get; set; }
        public int CurrentPage { get; set; }
        public int RowsPerPage { get; set; }

        public IList<object> Data { get; set; }
    }

    public class TableDataResponse<TEntity> : TableDataResponse
    {
        public IList<TEntity> Result { get; set; }

        public TableDataResponse(int? rowsCount, int currentPage, int rowsPerPage, IList<TEntity> result)
        {
            RowsCount = rowsCount;
            CurrentPage = currentPage;
            RowsPerPage = rowsPerPage;
            Result = result;
            Data = result.Cast<object>().ToList();
        }

        public TableDataResponse<TDto> Map<TDto>(IList<TDto> data)
        {
            return new TableDataResponse<TDto>(this.RowsCount, this.CurrentPage, this.RowsPerPage, data);
        }

        public TableDataResponse<TDto> Map<TDto>(Func<TEntity, TDto> mapFunc)
        {
            var mapped = this.Result.Select(mapFunc).ToList();

            return this.Map<TDto>(mapped);
        }
    }
}
