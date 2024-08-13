namespace DNDocs.Domain.Enums
{
    public enum BgProjectHealthCheckStatus
    {
        HttpGetOk = 1,
        HttpGetFail = 2,
        SystemFailedToInvokeGet = 3,
        IgnoredBecauseStatusNotActive = 4,
    }
}
