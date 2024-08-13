namespace DNDocs.Shared.Utils
{
    public static class IQueryableExtensions
    {
        //public static async Task<IList<TItem>> AToListAsync<TItem>(this IQueryable<TItem> query)
        //{
        //    var result = await query.ToListAsync();
        //    return result;
        //}
    }

    public static class StringExtensions
    {
        public static string StringJoin<T>(this IEnumerable<T> enumerable, string separator, Func<T, string> propSelector)
        {
            if (enumerable == null || !enumerable.Any()) return string.Empty;

            return string.Join(separator, enumerable.Select(t => propSelector(t)));
        }

        public static string StringJoin<T>(this IEnumerable<T> enumerable, string separator)
        {
            return StringJoin(enumerable, separator, t => t?.ToString() ?? string.Empty);
        }
    }
}
