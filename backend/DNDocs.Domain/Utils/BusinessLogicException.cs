namespace DNDocs.Domain.Utils
{
    public class BusinessLogicException : RobiniaException
    {
        public struct FieldError
        {
            public string FieldName;
            public string ErrorMessage;

            public FieldError(string fieldName, string error)
            {
                FieldName = fieldName;
                this.ErrorMessage = error;
            }

            public override string ToString()
            {
                return $"{FieldName ?? ""} : {ErrorMessage})";
            }
        }

        public string Error { get; set; }
        public FieldError[] FieldErrors { get; set; }

        public BusinessLogicException(string error)
        {
            this.Error = error;
        }

        public BusinessLogicException(IEnumerable<FieldError> errors)
        {
            this.FieldErrors = errors.ToArray();
        }
    }
}
