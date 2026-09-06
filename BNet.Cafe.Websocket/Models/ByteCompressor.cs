using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Websocket
{
    public static class ByteCompressor
    {
        public static byte[] Compress(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(
                    output,
                    CompressionLevel.Fastest,
                    true))
                {
                    gzip.Write(data, 0, data.Length);
                }

                return output.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            using (var input = new MemoryStream(data))
            using (var gzip = new GZipStream(
                input,
                CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                gzip.CopyTo(output);

                return output.ToArray();
            }
        }
    }
}