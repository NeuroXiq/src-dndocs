namespace DNDocs.Domain.Utils
{
    public class DNDomainException : Exception
    {
        public DNDomainException()
        {
        }

        public DNDomainException(string message) : base(message)
        {
        }
    }
}
