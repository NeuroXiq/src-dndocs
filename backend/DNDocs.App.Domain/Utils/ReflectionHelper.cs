using System.Reflection;
using System.Runtime.InteropServices;

namespace DNDocs.Domain.Utils
{
    ///
    /// <summary>
    /// Reflection will only work until class is disposed.
    /// Reflection will work only within assemblied that was loaded in content.
    /// Always use 'using (ReflectionHelper reflectionHelper = new ReflectionHelper(..) { reflectionhelper.load(); } )'
    /// temporary directory must always be created when using this class
    /// </summary>
    ///
    public class ReflectionHelper : IDisposable
    {
        public class LoadedAssembly
        {
            public Assembly Assembly { get; private set; }
            public AssemblyToLoad LoadedFrom { get; private set; }

            public LoadedAssembly(Assembly assembly, AssemblyToLoad loadedFrom)
            {
                Assembly = assembly;
                LoadedFrom = loadedFrom;
            }
        }

        private string temporaryDirectory;
        private AssemblyToLoad[] toLoad;
        private bool isLoaded = false;
        private bool isdisposed;

        public class AssemblyToLoad
        {
            /// Name must match real assembly name (real filename, must be equal to Assembly.Name, e.g. 'System.Runtime')
            public string AssemblyName;
            public byte[] AssemblyBytes;

            public AssemblyToLoad(string assemblyName, byte[] bytes)
            {
                if (!assemblyName.EndsWith(".dll"))
                    throw new Exception("assembly name not ends with dll");

                AssemblyName = assemblyName;
                AssemblyBytes = bytes;
            }
        }

        // public Assembly[] LoadedAssemblyReflectionReadOnly { get; private set; }
        public LoadedAssembly[] LoadedAssemblyReflectionReadOnly { get; private set; }


        public ReflectionHelper(AssemblyToLoad[] toLoad, string temporaryDirectoryName)
        {
            this.temporaryDirectory = temporaryDirectoryName;
            this.toLoad = toLoad;
        }

        public void Load()
        {
            if (this.isdisposed)
                throw new InvalidOperationException("Object is disposed and, no operations allowed");

            if (!Directory.Exists(temporaryDirectory))
                throw new ArgumentException("temporaryDirectory does not exists");

            if (toLoad == null || toLoad.Length == 0)
                throw new ArgumentException("toLoad is null or empty");

            // create temporary file on disk to load to pathassemblyresolver
            for (int i = 0; i < toLoad.Length; i++)
            {
                if (toLoad[i] == null)
                    throw new ArgumentNullException($"toLoad[{i}] is null");

                var ainfo = toLoad[i];

                if (ainfo.AssemblyBytes == null || ainfo.AssemblyBytes.Length == 0 ||
                    string.IsNullOrWhiteSpace(ainfo.AssemblyName))
                {
                    throw new ArgumentException($"toLoad[{i}] assemblyBytes null or length = 0, or assembly name empty");
                }

                for (int j = 0; j < ainfo.AssemblyName.Length; j++)
                {
                    int c =  ainfo.AssemblyName[j];
                    bool isvalid = (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || c == '.' || c == '-';

                    if (!isvalid)
                    {
                        throw new ArgumentException($"toLoad[{i}].AssemblyName = {ainfo.AssemblyName} is not valid file name");
                    }
                }
                    
                string fname = Path.Combine(temporaryDirectory, ainfo.AssemblyName);
                
                File.WriteAllBytes(fname, ainfo.AssemblyBytes);
            }

            List<LoadedAssembly> result = new List<LoadedAssembly>();

            // copy & paste from microsoft docs

            // Get the array of runtime assemblies.
            string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
            string[] toLoadAssemblies = Directory.EnumerateFiles(temporaryDirectory).ToArray();

            List<string> allPaths = new List<string>();
            allPaths.AddRange(runtimeAssemblies);
            allPaths.AddRange(toLoadAssemblies);

            // Create the list of assembly paths consisting of runtime assemblies and the inspected assembly.

            // Create PathAssemblyResolver that can resolve assemblies using the created list.
            var resolver = new PathAssemblyResolver(allPaths);

            var mlc = new MetadataLoadContext(resolver);

            foreach (var toLoadSingle in toLoad)
            {
                var assemblyReadyForReflection = mlc.LoadFromByteArray(toLoadSingle.AssemblyBytes);
                var loadedAssemblyInfo = new LoadedAssembly(assemblyReadyForReflection, toLoadSingle);

                result.Add(loadedAssemblyInfo);
            }

            this.LoadedAssemblyReflectionReadOnly= result.ToArray();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ReflectionHelper() => Dispose(false);

        protected virtual void Dispose(bool disposing)
        {
            if (isdisposed)
            {
                return;
            }

            if (disposing)
            {
                // cleanup manager (no managed right now)
            }

            Directory.Delete(this.temporaryDirectory, true);

            isdisposed = true;
        }
    }
}
