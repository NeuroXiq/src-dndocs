using DNDocs.Application.Shared;

namespace DNDocs.Application.Commands.Admin
{
    public class HardRecreateProjectCommand : Command<int>
    {
        public int ProjectId { get; set; }
    }
}
