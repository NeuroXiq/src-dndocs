using DNDocs.Api.DTO.Enum;
using DNDocs.Api.DTO.ProjectManage;

namespace DNDocs.Api.DTO.MyAccount
{
    public class BgJobViewModel
    {
        public int ProjectId { get; set; }
        public ProjectDto CreatedProject { get; set; }
        public string ProjectApiFolderUrl { get; set; }
        public int EstimateOtherJobsBeforeThis { get; set; }
        public double EstimateBuildTime { get; set; }
        public double EstimateStartIn { get; set; }
        public int StateDetails { get; set; }
        public int State { get; set; }
        public DateTime? LastDocfxBuildTime { get; set; }
    }
}
