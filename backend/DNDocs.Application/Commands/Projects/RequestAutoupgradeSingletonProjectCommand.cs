using DNDocs.Application.Shared;

namespace DNDocs.Application.Commands.Projects
{
    public class RequestAutoupgradeSingletonProjectCommand : Command<int>
    {
        public int ProjectId { get; set; }
    }
}
