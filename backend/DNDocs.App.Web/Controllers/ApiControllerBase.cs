using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DNDocs.Application.Shared;
using DNDocs.Web.Application.Validation;
using DNDocs.Api.DTO;
using System.Text.Json;

namespace DNDocs.Web.Controllers
{
    public class FieldError
    {
        public string FieldName { get; set; }
        public string ErrorMessage { get; set; }

        public FieldError(string fieldName, string errorMessage)
        {
            FieldName = fieldName;
            ErrorMessage = errorMessage;
        }
    }

    [ServiceFilter(typeof(RobiniaApiControllerActionFilter))]
    [Route("/api/[controller]/[action]")]
    [ApiController]
    public class ApiControllerBase : ControllerBase
    {
        public class ApiErrorResult : ApiResult
        {
            public string ErrorMessage { get; set; }
            public FieldError[] FieldErrors { get; private set; }

            public ApiErrorResult(string errorMessage, List<FieldError> fieldErrors, object data) : base(false, data)
            {
                ErrorMessage = errorMessage;
                FieldErrors = fieldErrors.ToArray();
            }
        }

        public class ApiSuccessResult : ApiResult
        {
            public ApiSuccessResult(object result) : base(true, result) { }
        }

        public class ApiResult
        {
            public bool Success { get; set; }
            public object Result { get; set; }

            public ApiResult(bool success, object result)
            {
                Success = success;
                Result = result;
            }
        }

        protected async Task<IActionResult> ApiResult2<TR>(Task<QueryResult<TR>> res)
        {
            var qr = Mapper.MapQR(await res);

            return RawJsonResult(qr);
        }

        protected async Task<IActionResult> ApiResult2<TR>(Task<CommandResult<TR>> res)
        {
            var cr = Mapper.MapCR(await res);

            return RawJsonResult(cr);
        }

        protected async Task<IActionResult> ApiResult2(Task<CommandResult> res)
        {
            var cr = Mapper.Map(await res);

            return RawJsonResult(cr);
        }

        public static IActionResult RawJsonResult(HandlerResultDto result)
        {
            // Response.StatusCode = (int)result.Code;
            var r = new ContentResult();
            r.ContentType = "application/json";
            r.StatusCode = result.Success ? 200 : 400;
            // r.Content = System.Text.Json.JsonSerializer.Serialize(result);
            // r.Content = JsonConvert.SerializeObject(result);
            r.Content = System.Text.Json.JsonSerializer.Serialize((object)result, new System.Text.Json.JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return r;
            // return new JsonResult(result);
        }
    }
}
