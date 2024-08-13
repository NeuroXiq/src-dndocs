using DNDocs.Api.DTO.ProjectManage;
using DNDocs.Api.DTO.Shared;

namespace DNDocs.Api.DTO.Admin
{
    public class AdminDashboardInfoDto
    {
        public int ProblemsCount { get; set; }
        public int UniqueVisitors24H { get; set; }
        public int UniqueVisitors7Days { get; set; }
        public IList<AppLogDto> LastLogs { get; set; }
        public IList<AppLogDto> LastHighPriorityLogs { get; set; }
        public int InMemoryQueuedLogsToSave { get; set; }
        public int AppLogsCount { get; set; }
        public int HttpLogsCount { get; set; }
        public TableDataDto<ProjectDto> Projects { get; set; }

        public bool BackgroundDoWorkIsRunning { get; set; }
        public bool BackgroundDoImportantWorkIsRunning { get; set; }
        public int BackgroundQueueHttpLogsCount { get; set; }
        public int BackgroundQueueAppLogsCount { get; set; }
        public string BackgroundStatusText { get; set; }
    }
}
