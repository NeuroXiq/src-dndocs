using System.ComponentModel.DataAnnotations;

namespace DNDocs.Web.Application.Validation
{
    public enum ValidateFilesType
    {
        DotNetAssemblyOrXmlDoc
    }

    public class ValidateFilesAttribute : RobiniaBaseValidationAttribute
    {
        public ValidateFilesType Type { get; set; }

        const long MaxLengthSumOfAllFiles = 14 * 1024 * 1024;

        public ValidateFilesAttribute()
        {
        }

        protected override ValidationResult Validate(object value, ValidationContext validationContext)
        {
            if (Type == ValidateFilesType.DotNetAssemblyOrXmlDoc)
            {
                return Validate_DotNetAssemblyOrXmlDoc(value, validationContext);
            }
            else
            {
                throw new Exception("internal must never happen");
            }
        }

        private ValidationResult Validate_DotNetAssemblyOrXmlDoc(object value, ValidationContext validationContext)
        {
            var filesArray = value as IList<IFormFile>;
            var singleFile = value as IFormFile;

            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (filesArray == null && singleFile == null)
            {
                throw new ArgumentException(
                    "ValidateFilesAttribute: something is wrong with object value." + 
                    "Object form value (expected IFormFile) is not null and cannot be casted to IFormFile and IList<IFormFile>." + 
                    "Is attribute applied for field of type 'IFormFile' or 'IList<IFormFile>'?");
            }

            if (filesArray != null && filesArray.Count == 0)
            {
                return ValidationResult.Success;
            }

            if (singleFile != null)
                filesArray = new List<IFormFile> { singleFile };

            long size = filesArray.Sum(f => f.Length);

            if (size > MaxLengthSumOfAllFiles)
            {
                return new ValidationResult(null);
            }

            List<string> errors = new List<string>();

            if (!filesArray.All(ValidateSingleFile_DotNetAssemblyOrXmlDoc))
                return new ValidationResult(null);

            return ValidationResult.Success;
        }

        bool ValidateSingleFile_DotNetAssemblyOrXmlDoc(IFormFile file)
        {
            if (file.Length > MaxLengthSumOfAllFiles)
                return false;

            var fileExt = Path.GetExtension(file.FileName);

            if (fileExt != ".dll" && fileExt != ".xml")
                return false;

            return true;
        }
    }
}
