using DNDocs.Application.Shared;

namespace DNDocs.Application.Commands.Admin
{
    public class DoBackgroundWorkCommand : Command
    {
        public bool ForceAll { get; set; }
        public bool ForceQueuedItems { get; set; }
        public bool ForceCheckHttpStatusForProjects { get; set; }
        public bool ForceGenerateSitemap { get; set; }
        public bool ForceRemoteTryItProjects { get; set; }

        public bool DoOnlyBgJobs { get; set; }
    }

    public class RequestDoBackgroundWorkCommand : Command
    {
        public bool ForceAll { get; set; }
        public bool ForceQueuedItems { get; set; }
        public bool ForceCheckHttpStatusForProjects { get; set; }

        public bool DoOnlyBgJobs { get; set; }
    }
}
