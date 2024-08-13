using DNDocs.Application.Shared;

namespace DNDocs.Application.Commands
{
    public class EditDocfxProjectCommand : Command
    {
        public class UpdateFileCommand
        {
            public string VPath { get; set; }
            public string NewContent { get; set; }
        }

        public class CreateDirectoryCommand
        {
            public string ParentDirectoryPath { get; set; }

            public CreateDirectoryCommand() { }
            public CreateDirectoryCommand(string path) { ParentDirectoryPath = path; }
        }

        public class MoveDirectoryCommand
        {
            public string VPathSrc { get; set; }
            public string VPathDest { get; set; }
        }

        public class RemoveDirectoryCommand
        {
            public string DirectoryPath { get; set; }
        }

        public class CreateFileCommand
        {
            public string ParentDirectory { get; set; }
            public string FileName { get; set; }
            public string Extension { get; set; }
        }

        public class MoveFileCommand
        {
            public string VPathSrc { get; set; }
            public string VPathDest { get; set; }
        }

        public class RemoveFileCommand
        {
            public string VPath { get; set; }
        }

        public int ProjectId { get; set; }

        public CreateDirectoryCommand CreateDirectory { get; set; }
        public MoveDirectoryCommand MoveDirectory { get; set; }
        public RemoveDirectoryCommand RemoveDirectory { get; set; }
        public CreateFileCommand CreateFile { get; set; }
        public MoveFileCommand MoveFile { get; set; }
        public RemoveFileCommand RemoveFile { get; set; }
        public UpdateFileCommand UpdateFile { get; set; }
    }
}
