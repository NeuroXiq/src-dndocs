export default interface BgJobViewModel {
    projectId: number,
    createdProject: any,
    projectApiFolderUrl: string,
    estimateOtherJobsBeforeThis: number,
    estimateBuildTime: number,
    estimateStartIn: number,
    stateDetails: number,
    state: number,
    lastDocfxBuildTime: string,
}