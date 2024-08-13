using DNDocs.Domain.Entity.App;
using DNDocs.Domain.ValueTypes;
using DNDocs.Api.DTO;
using DNDocs.Api.DTO.ProjectManage;

namespace DNDocs.Domain.ValueTypes
{
    public class AdminDashboardInfo
    {
        public int ProblemsCount;
        public int UniqueVisitors24H;
        public int UniqueVisitors7Days;
        public IList<AppLog> LastLogs;
        public IList<AppLog> LastHighPriorityLogs;
        public IList<BgJobDto> BgJobs { get; set; }
        public int InMemoryQueuedLogsToSave { get; internal set; }
        public int AppLogsCount { get; internal set; }
        public int HttpLogsCount { get; internal set; }
        public TableDataResponse<ProjectDto> Projects { get; set; }

        public AdminDashboardInfo(int problemsCount,
            int uniqueVisitors24h,
            IList<AppLog> lastLogs,
            IList<AppLog> lastHighPrioLogs)
        {
            ProblemsCount = problemsCount;
            UniqueVisitors24H = uniqueVisitors24h;
            LastLogs = lastLogs;
            LastHighPriorityLogs = lastHighPrioLogs;
        }
    }
}
