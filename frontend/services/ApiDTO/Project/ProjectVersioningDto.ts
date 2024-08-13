import NugetPackageDto from "./NugetPackageDto";
import ProjectVersionDto from "./ProjectVersionDto";

export default interface ProjectVersioningDto {
    id: number,
    projectName: string,
    projectWebsiteUrl: string,
    urlPrefix: string,
    gitDocsRepoUrl: string,
    gitDocsBranchName: string,
    gitDocsRelativePath: string,
    gitHomepageRelativePath: string,
    autoupgrage: boolean,
    nugetPackages: NugetPackageDto[] | null,
    projectVersions: ProjectVersionDto[] | null
}