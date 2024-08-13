export interface TableDataRequest {
    TableName: string,
    GetRowsCount: boolean,
    PageNo: number,
    RowsPerPage: number,
    Filters: Filter[],
    OrderBy: Order[]
}

export interface Filter {
    Column: string,
    Type: string,
    Value: string
}

export interface Order {
    Column: string,
    Dir: TableDataRequestOrderDir
}

export enum TableDataRequestOrderDir {
    Asc = 1,
    Desc = 2
}