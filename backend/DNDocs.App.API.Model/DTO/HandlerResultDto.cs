using DNDocs.Api.DTO.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.DTO
{
    public class HandlerResultDto
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public FieldErrorDto[] FieldErrors { get; set; }

        public HandlerResultDto() { }

        public HandlerResultDto(bool success, string errorMessage, IEnumerable<FieldErrorDto> fieldErrors)
        {
            Success = success;
            ErrorMessage = errorMessage;
            FieldErrors = fieldErrors?.ToArray();
        }
    }
}
