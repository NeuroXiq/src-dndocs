namespace DNDocs.Docs.Web.Shared
{
    public class DValidation
    {
        public static void ThrowISE(bool isthrow, string msg)
        {
            if (isthrow) throw new DInternalSystemException(msg);
        }

        public static void Throw(bool isThrow, string message)
        {
            if (isThrow)
            {
                throw new DValidationException(message);
            }
        }
    }
}
