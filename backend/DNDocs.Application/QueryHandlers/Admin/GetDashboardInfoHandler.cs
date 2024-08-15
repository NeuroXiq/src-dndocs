using DNDocs.Application.Application;
using DNDocs.Application.CommandHandlers;
using DNDocs.Application.Queries.Admin;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Repository;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Api.DTO.Admin;

namespace DNDocs.Application.QueryHandlers.Admin
{
    internal class GetDashboardInfoHandler : QueryHandlerA<GetDashboardInfoQuery, AdminDashboardInfoDto>
    {
        private IAppUnitOfWork uow;
        private IHttpLogRepository httpLogRepository;
        private IAppLogRepository appLogRepository;

        public GetDashboardInfoHandler(IAppUnitOfWork uow)
        {
            this.uow = uow;
            this.httpLogRepository = this.uow.HttpLogRepository;
            this.appLogRepository = this.uow.AppLogRepository;
        }

        protected override Task<AdminDashboardInfoDto> Handle(GetDashboardInfoQuery query)
        {
            var problems = 0;
            var uniqueVisitors24h = httpLogRepository.UniqueIP(DateTime.Now.AddDays(-1));
            var uniqueVisitors7days = httpLogRepository.UniqueIP(DateTime.Now.AddHours(-7));
            var lastLogs = appLogRepository.GetLastLogs(10, 0);
            var lastHighPriorityLogs = appLogRepository.GetLastLogs(200, 3);


            var result = new AdminDashboardInfoDto();

            result.UniqueVisitors7Days = uniqueVisitors7days;

            var bgactions = this.uow.GetSimpleRepository<BgJob>()
                .Query()
                .OrderBy(a => a.Status == Domain.Enums.BgJobStatus.InProgress ? 0 : 1)
                .ThenByDescending(a => a.QueuedDateTime)
                .ToList();

            var projects = this.uow.GetSimpleRepository<Project>()
                .GetTableData(new TableDataRequest(1, 999999999, false));

            result.InMemoryQueuedLogsToSave = -1;
            // result.AppLogsCount = this.uow.GetSimpleRepository<AppLog>().Query().Count();
            // result.HttpLogsCount = uow.GetSimpleRepository<HttpLog>().Query().Count();
            result.Projects = Mapper.Map(projects);

            result.BackgroundQueueAppLogsCount = DoBackgroundWorkHandler.Logs.Count;
            result.BackgroundQueueHttpLogsCount = DoBackgroundWorkHandler.HttpLogs.Count;
            result.BackgroundStatusText = DoBackgroundWorkHandler.StatusString;

            return Task.FromResult(result);
        }
    }
}
