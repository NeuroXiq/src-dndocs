using DNDocs.Application.Shared;

namespace DNDocs.Application.Commands
{
    public class DeleteProjectCommand : Command
    {
        public int ProjectId { get; set; }

        public DeleteProjectCommand(int id) { ProjectId = id; }
        public DeleteProjectCommand() { }
    }
}
