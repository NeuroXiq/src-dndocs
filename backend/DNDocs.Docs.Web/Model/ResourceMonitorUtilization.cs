namespace DNDocs.Docs.Web.Model
{
    public class ResourceMonitorUtilization
    {
        public int Id { get; set; }
        public double CpuUsedPercentage { get; set; }
        public ulong MemoryUsedInBytes { get; set; }
        public double MemoryUsedPercentage { get; set; }
        public DateTime DateTime { get; set; }

        public ResourceMonitorUtilization() { }

        public ResourceMonitorUtilization(
            double cpuUsedPercentage,
            ulong memoryUsedInBytes,
            double memoryUsedPercentage,
            DateTime dateTime)
        {
            CpuUsedPercentage = cpuUsedPercentage;
            MemoryUsedInBytes = memoryUsedInBytes;
            MemoryUsedPercentage = memoryUsedPercentage;
            DateTime = dateTime;
        }
    }
}
