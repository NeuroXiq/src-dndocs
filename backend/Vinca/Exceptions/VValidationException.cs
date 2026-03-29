namespace Vinca.Exceptions
{
    public class VValidationException : VHttpException
    {
        public VValidationException(string message) : base(System.Net.HttpStatusCode.BadRequest, message) { }
    }
}
