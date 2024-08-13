namespace DNDocs.Docs.Web.Web
{
    public class ApiEndpoint
    {
        public string Route;
        public Delegate Delegate;
        public HttpMethod HttpMethod;

        public ApiEndpoint(HttpMethod method, string route, Delegate action)
        {
            this.HttpMethod = method;
            Route = route;
            Delegate = action;
        }
    }
}
