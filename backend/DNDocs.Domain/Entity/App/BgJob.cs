
using DNDocs.Domain.Enums;
namespace DNDocs.Domain.Entity.App
{
    public class BgJob : Entity
    {
        public int Id { get; set; }
        public DateTime QueuedDateTime { get; set; }
        public DateTime? StartedDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public BgJobStatus Status { get; set; }
        public string DoWorkCommandType { get; set; }
        public string DoWorkCommandData { get; set; }
        public int ExecuteAsUserId { get; set; }
        public bool? CommandHandlerSuccess { get; set; }
        public string CommandHandlerResult { get; set; }
        public string Exception { get; set; }
        public string ExeThreadId { get; set; }
        public int? BuildsProjectId { get; set; }
        public User CreatedByUser { get; set; }

        public BgJob() { }

        public BgJob(string doworkCommandType, string doworkCommandData, int executeAsUserId)
        {
            QueuedDateTime = DateTime.UtcNow;
            StartedDateTime = null;
            CompletedDateTime = null;
            Status = BgJobStatus.WaitingToStart;
            ExecuteAsUserId = executeAsUserId;

            DoWorkCommandType = doworkCommandType;
            DoWorkCommandData = doworkCommandData;
        }

        public void SetCompleted(bool commandHandlerSuccess, string cmdHandlerResult)
        {
            CompletedDateTime = DateTime.UtcNow;
            Status = BgJobStatus.Completed;
            CommandHandlerSuccess = commandHandlerSuccess;
            CommandHandlerResult = cmdHandlerResult;
        }

        public void SetFailedToRun(string exception)
        {
            Status = BgJobStatus.FailedToStart;
            CompletedDateTime = DateTime.UtcNow;
            Exception = exception;
        }

        public void SetInProgress(string exeThreadId)
        {
            Status = BgJobStatus.InProgress;
            StartedDateTime = DateTime.UtcNow;
            ExeThreadId = exeThreadId;
        }

        public override string ToString()
        {
            return $"BgJob: Id: {Id}";
        }
    }
}
