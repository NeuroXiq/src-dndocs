using DNDocs.Application.Shared;

namespace DNDocs.Application.Commands.Projects
{
    internal class AutoupgradeSingletonProjectCommand : Command<int>, ICommandBgJob
    {
        public int ProjectId { get; set; }

        public AutoupgradeSingletonProjectCommand() { }

        public AutoupgradeSingletonProjectCommand(int projectId) { ProjectId = projectId; }

    }
}
