namespace DNDocs.Web.Application.RateLimit
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class RateLimitAttribute : Attribute
    {
        public string Id { get; set; }

        public RateLimitAttribute(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            Id = id;
        }
    }
}
