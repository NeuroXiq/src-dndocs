export default interface TableData<TData> {
    rowsCount: number
    pagesCount: number,
    currentPage: number,
    rowsPerPage: number,
    data: TData[]
}