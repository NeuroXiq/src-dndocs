namespace DNDocs.Domain.Service
{
    public interface IBlobDataService
    {
        bool IsValidDotNetAssemblyXmlDocumentation(string filename, byte[] data);

        bool IsValidDotNetDllAssembly(string filename, byte[] data);
    }
}
