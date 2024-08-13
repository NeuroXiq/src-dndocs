using DNDocs.Api.DTO.MyAccount;
using DNDocs.Application.Queries.MyAccount;
using DNDocs.Application.Shared;
using DNDocs.Application.Utils;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Shared.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.QueryHandlers.MyAccount
{
    internal class GetBgJobHandler : QueryHandlerA<GetBgJobQuery, BgJobViewModel>
    {
        private IAppUnitOfWork uow;
        private DNDocsSettings settings;

        public GetBgJobHandler(
            IAppUnitOfWork uow,
            IOptions<DNDocsSettings> options
            )
        {
            this.uow = uow;
            this.settings = options.Value;
        }


        protected override async Task<BgJobViewModel> Handle(GetBgJobQuery query)
        {
            return null;
            //var job = await uow.BgJobRepository.GetLatestBuildsProjectIdAsync(query.ProjectId);
            //var project = await uow.ProjectRepository.GetByIdCheckedAsync(query.ProjectId);

            //if (job == null) Validation.ThrowEntityNotFoundException<BgJob>($"BgJob by project id: '{query.ProjectId}' was not found");

            //var result = new BgJobViewModel
            //{
            //    EsitamedTimeWillExecuteSeconds = (int)-1,
            //    EstimatedTimeToStartSeconds = (int)(-1),
            //    EstimateOtherJobsBeforeThis = -1,

            //    BgJobId = job.Id,
            //    BgJobStatus = (DNDocs.Api.DTO.Enum.BgJobStatus)job.Status,
            //    CommandHandlerSuccess = job.CommandHandlerSuccess,
            //    ProjectApiFolderUrl = Helpers.GetProjectUrl(project, settings),
            //    CommandHandlerErrorMessage = job.CommandHandlerResult != null ? JsonConvert.DeserializeObject<HandlerResult>(job.CommandHandlerResult)?.ErrorMessage : null
            //};

            //return result;
        }
    }
}
