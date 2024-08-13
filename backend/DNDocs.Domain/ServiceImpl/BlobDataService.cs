using DNDocs.Domain.Service;
using DNDocs.Domain.Utils;
using System.Xml.Linq;

namespace DNDocs.Domain.ServiceImpl
{
    public class BlobDataService : IBlobDataService
    {
        private IAppManager appManager;

        public BlobDataService(IAppManager appManager)
        {
            this.appManager = appManager;
        }

        public bool IsValidDotNetAssemblyXmlDocumentation(string filename, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(filename) ||
                Path.GetExtension(filename) != ".xml" ||
                data == null ||
                data.Length == 0)
            {
                return false;
            }

            try
            {
                XDocument.Load(new MemoryStream(data));
            }
            catch
            {
                return false;
            }

            return true;
        }

        //public static void asdf()
        //{
        //    var files = Directory.GetFiles(@"C:\Users\mweglarz\Desktop\Robinia\dot_net_core_dll_assemblies\")
        //        .Where(f => f.EndsWith(".dll"))
        //        .ToArray();

        //    files = new string[] { @"C:\\Users\\mweglarz\\Desktop\\Robinia\\dot_net_core_dll_assemblies\\System.ServiceModel.Duplex.dll" };

        //    foreach (var filepath in files)
        //    {
        //        var data = File.ReadAllBytes(filepath);

        //        try
        //        {
        //            var x = ReflectionHelper.CreateReflectionReadOnlyFromBytes(data).AssemblyReflectionReadOnly;
        //            var q = x.GetTypes();
        //            var w = x.GetTypes().Where(t => t.IsClass).ToArray();
        //            var t = x.GetTypes().Where(t => t.IsClass).SelectMany(z => z.GetMethods()).ToArray();
        //            var waaa = 6;
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine("\r\n=======\r\n");
        //            Console.WriteLine(e.Message);
        //        }
                
        //    }

        //    var y = "";

        //    //// var data = File.ReadAllBytes(@"C:\Users\mweglarz\Desktop\Robinia\dot_net_core_dll_assemblies\mscorlib.dll");
        //    //var data = File.ReadAllBytes(@"C:\Users\mweglarz\Desktop\Robinia\dot_net_other_dll\BouncyCastle.Crypto.dll");

        //    //// Get the array of runtime assemblies.
        //    //string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

        //    //// Create the list of assembly paths consisting of runtime assemblies and the inspected assembly.
        //    //var paths = new List<string>(runtimeAssemblies);

        //    //// Create PathAssemblyResolver that can resolve assemblies using the created list.
        //    //var resolver = new PathAssemblyResolver(paths);

            
        //}

        public bool IsValidDotNetDllAssembly(string filename, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(filename) ||
                (Path.GetExtension(filename) != ".dll") ||
                data == null ||
                data.Length == 0)
            {
                return false;
            }

            throw new NotImplementedException("throw because commented below");

            try
            {
                var toloadSingle = new ReflectionHelper.AssemblyToLoad(filename, data);
                var toLoad = new ReflectionHelper.AssemblyToLoad[] { toloadSingle };
                // var tempFolder = appManager.CreateTempFolder();
                // 
                // using (var rh = new ReflectionHelper(toLoad, tempFolder.OSFullPath))
                // {
                //     rh.Load();
                // }
            }
            catch (Exception e)
            {
                var x = e;
                return false;
            }

            return true;
        }
    }
}
