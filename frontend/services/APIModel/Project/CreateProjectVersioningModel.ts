import NugetPackageModel from "../Shared/NugetPackageModel";

export default interface CreateProjectVersioningModel {
    projectName: string,
    projectWebsiteUrl: string,
    urlPrefix: string,
    gitDocsRepoUrl: string,
    gitDocsBranchName: string,
    gitDocsRelativePath: string,
    gitHomepageRelativePath: string,
    autoupgrage: boolean,
    nugetPackages: NugetPackageModel[] | null
}