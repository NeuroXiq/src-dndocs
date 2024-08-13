using Microsoft.AspNetCore.Mvc;
using DNDocs.Application.Commands.Admin;
using DNDocs.Application.Queries;
using DNDocs.Application.Queries.Admin;
using DNDocs.Application.Shared;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Web.Application;
using DNDocs.Web.Application.Authorization;
using DNDocs.Web.Models.Admin;
using DNDocs.Api.DTO.Admin;
using DNDocs.Api.Admin;

namespace DNDocs.Web.Controllers
{
    [Authorization(PolicyData = PolicyData.Administrator)]
    public class AdminController : ApiControllerBase
    {
        private IQueryDispatcher qd;
        private ICommandDispatcher cd;
        private IWebUser webUser;
        private IUnitOfWorkFactory uowf;
        private IAppManager mgr;
        private IRobiniaResources resources;

        public AdminController(IAppManager mgr,
            IUnitOfWorkFactory uowf,
            IRobiniaResources resources,
            IWebUser webUser,
            ICommandDispatcher cd,
            IQueryDispatcher qd)
        {
            this.qd = qd;
            this.cd = cd;
            this.webUser = webUser;
            this.uowf = uowf;
            this.mgr = mgr;
            this.resources = resources;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardInfo()
        {
            return await ApiResult2(this.qd.DispatchAsync(new GetDashboardInfoQuery()));
        }

        [HttpPost]
        public async Task<IActionResult> GetTableData([FromBody] TableDataRequest model)
        {
            return await ApiResult2(qd.DispatchAsync(new GetTableDataQuery(model)));
        }

        //
        // POST
        //

        [HttpPost]
        public async Task<IActionResult> HardRecreateProject([FromBody] int projectid)
        {
            return await ApiResult2(cd.DispatchAsync(new HardRecreateProjectCommand { ProjectId = projectid }));
        }
 
        [HttpPost]
        public async Task<IActionResult> DoBackgroundWorkNow([FromBody] DoBackgroundWorkNowModel model)
        {
            var cmd = new RequestDoBackgroundWorkCommand
            {
                ForceAll = model.ForceAll,
                ForceQueuedItems = model.ForceQueuedItems,
                ForceCheckHttpStatusForProjects = model.ForceCheckHttpStatusForProjects,
            };
            
            return await ApiResult2(cd.DispatchAsync(cmd));
        }

        [HttpPost]
        public IActionResult AttachTenant(AttachTenantModel model)
        {
            throw new NotImplementedException();
            // Mapper.Map(cd.Dispatch(new AttachTenantCommand() { ProjectId = projectid, Zip = zip }))
            // var blobfile = mapper.Map<FormFileDto>(model.File);
            // 
            // return ApiResult2(adminService.AttachTenant(model.TenantId, blobfile.ByteData));
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteRawSql([FromBody] ExecuteRawSqlModel model)
        {
            return await ApiResult2(qd.DispatchAsync(new ExecuteRawSqlQuery(model.DbName, model.Mode, model.SqlCode)));
        }

        //[HttpPost]
        //public IActionResult CreateNewProject(CreateProjectModel model)
        //{
        //    throw new NotImplementedException();
        //    // adminCmdService.RequestProject(
        //    //     model.ProjectName, model.Description,
        //    //     model.GitHubUrl, model.RobiniaUrlPrefix,
        //    //     mapper.Map<IList<BlobDataInfo>>(model.BlobFiles),
        //    //     -1);

        //    return ApiResultOk(gOkMsg: resources["ProjectCreatedSuccessfully"]);
        //}

        //public IActionResult GetDashboardInfo()
        //{
        //    return Json(adminQueryService.GetAdminDashboardInfo());
        //}
    }
}