namespace Vinca.Exceptions
{
    public class VHttpException : Exception
    {
        public int StatusCode { get; set; }
        public string Error { get; set; }

        public VHttpException(int statusCode, string message) : base(message)
        {
            StatusCode = (int)statusCode;
            Error = message;

#if DEBUG
            if ((int)statusCode < 100 || (int)statusCode > 600)
            {
                throw new ArgumentException("debug warning: status code not in valid range");
            }
#endif
        }

        public VHttpException(System.Net.HttpStatusCode statusCode, string message) : this((int)statusCode, message) { }
    }
}
