using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DNDocs.Application.Shared;
using DNDocs.Domain.Utils;
using DNDocs.Web.Controllers;
using DNDocs.Api.DTO;

namespace DNDocs.Web.Application.Validation
{

    public class RobiniaApiControllerActionFilter : IActionFilter, IExceptionFilter
    {
        private IRobiniaResources resources;

        public RobiniaApiControllerActionFilter(IRobiniaResources robiniaRes)
        {
            this.resources = robiniaRes;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid) return;

            context.Result = new BadRequestObjectResult(context.ModelState);

            var modelStateWithAnyErrors = context.ModelState
                .Where(kvpair => kvpair.Value.Errors.Any())
                .ToDictionary(s => s.Key, s => s.Value.Errors.ToArray());

            var fieldErrors = modelStateWithAnyErrors
                .Select(fieldInfo =>
            {
                var name = fieldInfo.Key;
                var fieldErrors = fieldInfo.Value.Select(modelError => modelError.ErrorMessage).ToArray();
                var error = string.Join(",", fieldErrors);

                return new FieldError(name, error);
            }).ToList();

            context.Result = KnownErrorsResult(null, fieldErrors);
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception as BusinessLogicException;
            
            if (exception == null) return;

            HandleErrors(exception, out var error, out var fieldErrors);

            context.Result = KnownErrorsResult(error, fieldErrors);
        }

        private IActionResult KnownErrorsResult(string errorMessage, List<FieldError> fieldErrors)
        {
            var filedErrorDtos = fieldErrors
                .Select(f => new FieldErrorDto(f.FieldName, f.ErrorMessage))
                .ToArray();

            var result = new CommandResultDto(false, errorMessage, filedErrorDtos);

            // return new JsonResult(result);
            return ApiControllerBase.RawJsonResult(result);
        }

        private void HandleErrors(BusinessLogicException e, out string error, out List<FieldError> fieldErrors)
        {
            error = resources[e.Error ?? ""];
            fieldErrors = new List<FieldError>();

            if (e.FieldErrors != null)
            {
                foreach (var fieldError in e.FieldErrors)
                {
                    var ferrors = resources[fieldError.ErrorMessage];
                    var fname = fieldError.FieldName;

                    fieldErrors.Add(new FieldError(fname, ferrors));
                }
            }
        }
    }
}
