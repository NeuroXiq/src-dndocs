using System.ComponentModel.DataAnnotations;

namespace DNDocs.Web.Application.Validation
{
    public abstract class RobiniaBaseValidationAttribute : ValidationAttribute
    {
        private IRobiniaResources resources;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result =  Validate(value, validationContext);

            if (result == ValidationResult.Success) return result;

            this.resources = validationContext.GetService<IRobiniaResources>();
            string errorToLocalize = result.ErrorMessage == null ? base.ErrorMessage : result.ErrorMessage;

            return new ValidationResult(LocalizeError(errorToLocalize));
        }

        protected abstract ValidationResult Validate(object value, ValidationContext validationContext);

        protected string LocalizeError(string msgToLocalize) => resources[msgToLocalize];
    }
}
