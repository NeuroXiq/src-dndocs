export default interface ProjectVersionDto {
    id: number,
    gitTag: string,
    projectId: number,
    projectVersioningId: number,
    projectUrlPrefix: string
}