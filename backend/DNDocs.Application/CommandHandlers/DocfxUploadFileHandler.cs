//using DNDocs.Application.Commands;
//using DNDocs.Application.Shared;
//using DNDocs.Domain.Utils;
//using DNDocs.Domain.Utils.Docfx;
//using System.Text;

//namespace DNDocs.Application.CommandHandlers
//{
//    internal class DocfxUploadFileHandler : CommandHandler<DocfxUploadFileCommand>
//    {
//        private IDocfxManager docfxManager;
//        private ICurrentUser user;

//        public DocfxUploadFileHandler(ICurrentUser user,
//            IDocfxManager docfxManager)
//        {
//            this.docfxManager = docfxManager;
//            this.user = user;
//        }

//        public override void Handle(DocfxUploadFileCommand command)
//        {
//            user.AuthorizationProjectManage(command.ProjectId);
//            docfxManager.Open(command.ProjectId);

//            string filename = Path.GetFileNameWithoutExtension(command.File.FileName);
//            filename = NormalizeFilenameOrPath(filename);
//            string vpath = NormalizeFilenameOrPath(command.VPath);
//            string ext = Path.GetExtension(command.File.FileName)?.TrimStart('.');

//            docfxManager.ContentCreateFile(vpath, filename, ext);
//            docfxManager.ContentUpdateFile($"{vpath}/{filename}.{ext}", Encoding.UTF8.GetString(command.File.ByteData));

//            docfxManager.Commit();
//        }

//        private string NormalizeFilenameOrPath(string originalName)
//        {
//            var commonsToReplace = new char[] { '(', ')', ' ', };
            
//            var filename = originalName.ToLower();
            
//            foreach (var c in commonsToReplace) filename = filename.Replace(c, '-');

//            return filename;
//        }
//    }
//}
