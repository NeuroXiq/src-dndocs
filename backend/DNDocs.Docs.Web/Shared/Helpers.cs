using System.IO.Compression;
using Vinca.Exceptions;

namespace DNDocs.Docs.Web.Shared
{
    public class Helpers
    {
        public static byte[] BrotliCompress(byte[] input, ref byte[] reusableTempCompressBuffer)
        {
            int min = input.Length;
            if (min < 8000) min = 8000; // not sure what min buffer set?

            // make 100% sure will work, this buffer is much more big (probably) than needed
            // question what to do with this to make this always work? (to do not create too small buffer?)
            if (reusableTempCompressBuffer.Length < min) reusableTempCompressBuffer = new byte[min];

            if (BrotliEncoder.TryCompress(input, reusableTempCompressBuffer, out var afterCompressLen))
            {
                byte[] compressedData = new byte[afterCompressLen];
                Buffer.BlockCopy(reusableTempCompressBuffer, 0, compressedData, 0, afterCompressLen);
                return compressedData;
            }
            else
            {
                VValidate.AppEx("failed to compress brotli");
                throw new Exception();
            }
        }
    }
}
