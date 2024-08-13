namespace DNDocs.Api.DTO
{
    public class DocfxProjectQueryResultDto
    {
        public class FileContentResultDto
        {
            public FileContentResultDto(string vPath, string content)
            {
                VPath = vPath;
                Content = content;
            }

            public string VPath { get; set; }
            public string Content { get; set; }
        }

        public int ProjectId { get; set; }

        public FileContentResultDto FileContentResult { get; set; }
    }
}
