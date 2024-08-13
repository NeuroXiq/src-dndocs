import { BgProjectHealthCheckStatus } from "../Enum/BgProjectHealthCheckStatus"
import { ProjectStatus } from "../Enum/ProjectStatus"
import ProjectNugetPackageDto from "./NugetPackageDto"

export default interface ProjectDto {
    id: number,
    uuid: string,
    projectName: string,
    urlPrefix: string,
    description: string,
    githubUrl: string,
    comment: string,
    status: ProjectStatus,
    createdOn: string,
    lastModifiedOn: string
    lastDocfxBuildTime: string | null,
    lastDocfxBuildErrorLog: string | null,
    lastDocfxBuildErrorDateTime: string | null,
    projectNugetPackages: ProjectNugetPackageDto[],
    bgHealthCheckHttpGetStatus: BgProjectHealthCheckStatus | null,
    bgHealthCheckHttpGetDateTime: string | null,
    nupkgAutorebuildLastDateTime: string | null,

    githubMdRepoUrl: string | null,
    githubMdBranchName: string | null,
    githubMdRelativePathDocs: string | null,
    githubMdRelativePathReadme: string | null,
    githubMdCommitHashDocs: string | null,
    githubMdCommitHashReadme: string | null,
    psAutoRebuild: boolean,
    
}