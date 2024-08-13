//using DNDocs.Application.Commands;
//using DNDocs.Application.Shared;
//using DNDocs.Domain.Utils;
//using DNDocs.Domain.Utils.Docfx;

//namespace DNDocs.Application.CommandHandlers
//{
//    internal class DocfxEditProjectHandler : CommandHandler<EditDocfxProjectCommand>
//    {
//        private ICurrentUser currentUser;
//        private IDocfxManager docfxManager;

//        public DocfxEditProjectHandler(
//            IDocfxManager docfxManager,
//            ICurrentUser currentUser)
//        {
//            this.currentUser = currentUser;
//            this.docfxManager = docfxManager;
//        }

//        public override void Handle(EditDocfxProjectCommand command)
//        {
//            this.currentUser.AuthorizationProjectManage(command.ProjectId);
//            this.docfxManager.Open(command.ProjectId);

//            if (command.RemoveDirectory == null && command.RemoveFile == null)
//            {
//                var totalDirs = Directory.GetDirectories(docfxManager.RootDirectory, "*", SearchOption.AllDirectories).Count();

//                Validation.ThrowError(totalDirs > 50, "Max directories count (including not managable) is 50. Cannot edit project. Remove directories");
//            }

//            if (command.CreateDirectory != null)
//            {
//                CreateDirectory(command.CreateDirectory);
//            }
//            else if (command.CreateFile != null)
//            {
//                CreateFile(command.CreateFile);
//            }
//            else if (command.UpdateFile != null)
//            {
//                var uf = command.UpdateFile;
//                docfxManager.ContentUpdateFile(uf.VPath, uf.NewContent);
//            }
//            else if (command.RemoveDirectory != null)
//            {
//                docfxManager.ContentDeleteDirectory(command.RemoveDirectory.DirectoryPath);
//            }
//            else if (command.RemoveFile != null)
//            {
//                docfxManager.ContentRemoveFile(command.RemoveFile.VPath);
//            }
//            else if (command.MoveDirectory != null)
//            {
//                docfxManager.ContentMoveDirectory(command.MoveDirectory.VPathSrc, command.MoveDirectory.VPathDest);
//            }
//            else if (command.MoveFile != null)
//            {
//                docfxManager.ContentMoveFile(command.MoveFile.VPathSrc, command.MoveFile.VPathDest);
//            }
//            else throw new NotImplementedException();

//            this.docfxManager.Commit();
//        }

//        private void CreateFile(EditDocfxProjectCommand.CreateFileCommand addFile)
//        {
//            docfxManager.ContentCreateFile(addFile.ParentDirectory, addFile.FileName, addFile.Extension);
//        }

//        private void CreateDirectory(EditDocfxProjectCommand.CreateDirectoryCommand createDirectory)
//        {
//            docfxManager.ContentCreateDirectory(createDirectory.ParentDirectoryPath);
//        }
//    }
//}
