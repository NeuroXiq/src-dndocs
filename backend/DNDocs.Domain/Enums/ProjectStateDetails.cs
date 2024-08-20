namespace DNDocs.Domain.Enums
{
    public enum ProjectStateDetails
    {
        Other = 1,
        WaitingToBuild = 2,
        Building = 3,
        BuildFailed = 4,
        BlockedByOwner = 5,
        Ready = 6,
        ManuallyCancelled = 7,
    }
}