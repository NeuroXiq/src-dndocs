namespace DNDocs.Domain.Enums
{
    public enum NugetOrgProjectState
    {
        Online = 1,
        WaitingToBuild = 2,
        Building = 3,
        BuildFailed = 4,
        BlockedByOwner = 5
    }
}