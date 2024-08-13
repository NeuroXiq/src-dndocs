import NugetPackageModel from "../Shared/NugetPackageModel";

export default interface CreateProjectVersionModel {
    gitTagName: string,
    nugetPackages: NugetPackageModel[]
}