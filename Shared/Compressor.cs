using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace OpenWeather
{
    public static class Compressor
    {
        public static byte[] Pack<T>(T o)
        {
            var b = JsonSerializer.SerializeToUtf8Bytes(o);
            using MemoryStream ms = new MemoryStream();
            using DeflateStream ds = new DeflateStream(ms, CompressionLevel.Optimal);
            ds.Write(b);
            ds.Flush();

            return ms.ToArray();
        }
        public static T Unpack<T>(byte[] b)
        {
            if (b is null) return default(T);
            using MemoryStream ms = new MemoryStream(b);
            using DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress);
            using MemoryStream oms = new MemoryStream();
            ds.CopyTo(oms);

            return JsonSerializer.Deserialize<T>(oms.ToArray());
        }
    }
}
