export default interface ApiResult<T> {
    result: T,
    error: string | null,
    fieldErrors: any
}