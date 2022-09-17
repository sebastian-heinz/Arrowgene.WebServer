using System;
using System.IO;
using System.Threading.Tasks;

namespace Arrowgene.WebServer
{
    internal class Util
    {
        /// <summary>
        ///     Read a stream till the end and return the read bytes.
        /// </summary>
        public static async Task<byte[]> ReadAsync(Stream stream)
        {
            var bufferSize = 1024;
            var buffer = new byte[bufferSize];
            var result = new byte[0];
            var offset = 0;
            var read = 0;
            while ((read = await stream.ReadAsync(buffer, 0, bufferSize)) > 0)
            {
                var newSize = offset + read;
                var temp = new byte[newSize];
                Buffer.BlockCopy(result, 0, temp, 0, offset);
                Buffer.BlockCopy(buffer, 0, temp, offset, read);
                result = temp;
                offset += read;
            }

            return result;
        }
    }
}