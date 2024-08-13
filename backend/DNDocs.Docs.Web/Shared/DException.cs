namespace DNDocs.Docs.Web.Shared
{
    public class DException : Exception
    {
        public DException(string msg) : base(msg) { }
    }

    public class DInternalSystemException : DException
    {
        public DInternalSystemException(string msg) : base(msg) { }
    }

    public class DUnauthorizedException : DException
    {
        public DUnauthorizedException() : base("Unauthorized") { }

        public DUnauthorizedException(string msg) : base(msg)
        {
        }
    }
}
