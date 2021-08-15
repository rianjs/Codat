using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace Connector
{
    public static class CompressionUtils
    {
        /// <summary>
        /// </summary>
        /// <param name="compressedBytes"></param>
        /// <param name="originalSize">if known</param>
        /// <returns></returns>
        public static byte[] DecompressBytes(byte[] compressedBytes, int originalSize = 0)
        {
            const int defaultSize = 4096;
            var knownSize = originalSize > 0;
            var bufferSize = knownSize
                ? originalSize
                : defaultSize;

            var buffer = new byte[bufferSize];

            using (var ms = new MemoryStream(compressedBytes))
            using (var decompressor = new GZipStream(ms, CompressionMode.Decompress))
            using (var destMemory = new MemoryStream())
            {
                if (knownSize)
                {
                    decompressor.Read(buffer, 0, bufferSize);
                    destMemory.Write(buffer, 0, bufferSize);
                    return destMemory.ToArray();
                }

                var readBytes = 0;
                do
                {
                    readBytes = decompressor.Read(buffer, 0, bufferSize);
                    if (readBytes > 0)
                    {
                        destMemory.Write(buffer, 0, readBytes);
                    }
                }
                while (readBytes > 0);
                return destMemory.ToArray();
            }
        }

        public static byte[] CompressBytes(byte[] data)
        {
            using (var source = new MemoryStream(data))
            using (var destination = new MemoryStream())
            {
                using (var compressor = new GZipStream(destination, CompressionMode.Compress))
                {
                    source.CopyTo(compressor);
                }
                return destination.ToArray();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encoding">UTF8 will be used if left null</param>
        /// <returns></returns>
        public static byte[] CompressString(string s, Encoding encoding = null)
        {
            var activeEncoder = encoding ?? Encoding.UTF8;
            var asBytes = activeEncoder.GetBytes(s);
            return CompressBytes(asBytes);
        }
        
        public static byte[] ToJsonSerializedGzipBytes<T>(T value, JsonSerializerSettings jsonSettings)
        {
            if (value is null)
            {
                return null;
            }

            if (jsonSettings is null)
            {
                throw new NullReferenceException(nameof(jsonSettings));
            }
            
            var serializer = JsonSerializer.Create(jsonSettings);
            
            using (var ms = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(ms, CompressionMode.Compress, leaveOpen: true))
                using (var textWriter = new StreamWriter(gzipStream))
                using (var jsonWriter = new JsonTextWriter(textWriter))
                {
                    serializer.Serialize(jsonWriter, value);
                }
                return ms.ToArray();
            }
        }
        
        public static T FromJsonSerializedGzipBytes<T>(byte[] jsonSerializedGzipBytes, JsonSerializerSettings jsonSettings)
        {
            if (jsonSerializedGzipBytes is null || jsonSerializedGzipBytes.Length == 0)
            {
                return default;
            }

            if (jsonSettings is null)
            {
                throw new NullReferenceException(nameof(jsonSettings));
            }

            var serializer = JsonSerializer.Create(jsonSettings);
            
            using (var ms = new MemoryStream(jsonSerializedGzipBytes))
            using (var gzipStream = new GZipStream(ms, CompressionMode.Decompress))
            using (var textReader = new StreamReader(gzipStream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                return serializer.Deserialize<T>(jsonReader);
            }
        }
    }
}