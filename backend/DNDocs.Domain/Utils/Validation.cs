using DNDocs.Shared.Utils;
using System.Runtime.CompilerServices;

namespace DNDocs.Domain.Utils
{
    public class Validation
    {
        public static void ThrowEntityNotFoundException<TEntity>(int id)
        {
            ThrowEntityNotFoundException<TEntity>(id.ToString());
        }

        public static void AppEx(bool condition, string msg)
        {
            if (condition) throw new AppException(msg);
        }

        public static void EnumDefined<T>(T value, string argname) where T : System.Enum
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new RobiniaException($"Enum value for '{typeof(T).Name}' is not defined, provided value: {value}, arg name: {argname}");
            }
        }

        public static void AppArgStringNotEmpty(string value, string argname)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new RobiniaException($"string '{value}' is null or white space");
            }
        }

        public static void ArgNotNull(object arg, string argname)
        {
            if (arg == null)
            {
                string msg = $"{argname} is null";

                throw new RobiniaException(msg);
            }
        }

        public static void AppThrowArg(bool shouldThrow, string argName, string msg)
        {
            if (shouldThrow)
            {
                throw new RobiniaException($"{msg}. Argument Name: {argName}");
            }
        }

        public static void AppThrowArg(bool shouldThrow, string argName)
        {
            AppThrowArg(shouldThrow, argName, $"Invalid value of '{argName}' argument.");
        }

        public static void ThrowEntityNotFoundException<TEntity>(string id)
        {
            throw new BusinessLogicException($"{typeof(TEntity).Name} with id {id} was not found");
        }

        public static void ThrowError(string message)
        {
            throw new BusinessLogicException(message);
        }

        public static void ThrowError(bool shouldThrow, string msg)
        {
            if (shouldThrow) ThrowError(msg);
        }

        public static void ThrowFieldError(string fieldName, string messageKey, bool conditionShouldThrow)
        {
            ThrowFieldError(conditionShouldThrow, fieldName, messageKey);
        }

        public static void ThrowFieldError(bool conditionShouldThrow, string fieldName, string messageKey)
        {
            if (conditionShouldThrow)
                ThrowFieldErrorCore(fieldName, messageKey);
        }

        public static void FieldErrorP(bool condition, object property, string msg, [CallerArgumentExpression("property")] string propertyName = "")
        {
            ThrowFieldError(condition, propertyName, msg);
        }

        //

        public static void NotStringIsNullOrWhiteSpace(string value, string msg = null, [CallerArgumentExpression("value")] string propertyName = "")
        {
            ThrowFieldError(string.IsNullOrWhiteSpace(value), propertyName, msg ?? "Value cannot be null or white space");
        }

        //

        public static void ThrowFieldErrorCore(string fieldName, string messageKey, bool normalizeFieldName = true)
        {
            if (normalizeFieldName && fieldName?.Contains(".") == true)
            {
                fieldName = fieldName.Trim().Split('.').Skip(1).StringJoin(".");
            }

            var fieldErrors = new List<BusinessLogicException.FieldError>()
            {
                new BusinessLogicException.FieldError(fieldName, messageKey)
            };

            var e = new BusinessLogicException(fieldErrors);

            throw e;
        }

        public static void ThrowGlobalError(string msg)
        {
            throw new BusinessLogicException(msg);
        }
    }
}
