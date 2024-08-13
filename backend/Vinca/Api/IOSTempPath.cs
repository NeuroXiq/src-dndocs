
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Api
{
    /// <summary>
    /// temp directory or temp file
    /// </summary>
    public interface IOSTempPath : IDisposable
    {
        string OSFullPath { get; }
    }

    internal class OSTempPath : IDisposable, IOSTempPath
    {
        public enum Type
        {
            File,
            Dir
        }

        private bool disposed;
        private Type type;
        string osPath;
        public string OSFullPath
        {
            get
            {
                if (disposed) throw new InvalidOperationException("Cannot use tempfolder because is disposed (deleted)");
                return osPath;
            }
        }

        public OSTempPath(string fullOsPath, Type type)
        {
            osPath = fullOsPath;
            disposed = false;
            this.type = type;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~OSTempPath() => Dispose(false);

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposed) return;

                if (disposing)
                {
                    // dispose managed objects
                }

                if (type == Type.Dir && Directory.Exists(osPath)) Directory.Delete(osPath, true);
                else if (File.Exists(osPath)) File.Delete(osPath);
            }
            catch
            {

            }
            finally
            {
                this.disposed = true;
            }
        }
    }
}
