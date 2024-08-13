export default interface NugetPackageDto {
    id: number,
    title: string ,
    identityVersion: string,
    identityId: string,
    dateTimeOffset?: string,
    projectUrl?: string,
    packageDetailsUrl?: string ,
    isListed: boolean,
    projectId: number
}