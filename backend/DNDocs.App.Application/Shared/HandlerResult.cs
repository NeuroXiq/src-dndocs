using DNDocs.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Shared
{
    public class HandlerResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public BusinessLogicException.FieldError[] FieldErrors { get; set; }

        public HandlerResult(bool success, string error, BusinessLogicException.FieldError[] fieldError)
        {
            Success = success;
            ErrorMessage = error;
            FieldErrors = fieldError;
        }
    }
}
