using DNDocs.Application.Shared;
using DNDocs.Api.DTO;

namespace DNDocs.Application.Commands
{
    public class DocfxUploadFileCommand : Command
    {
        public int ProjectId { get; set; }
        public string VPath { get; set; }
        public FormFileDto File { get; set; }

        public DocfxUploadFileCommand() { }

        public DocfxUploadFileCommand(int projectid, string vpath, FormFileDto file)
        {
            ProjectId = projectid;
            VPath = vpath;
            File = file;
        }
    }
}
