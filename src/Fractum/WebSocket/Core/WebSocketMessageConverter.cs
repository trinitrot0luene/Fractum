using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Fractum.Entities.WebSocket;
using Newtonsoft.Json;

namespace Fractum.WebSocket.Core
{
    /// <summary>
    ///     Decompresses Gateway messages to plaintext.
    ///     Credit to Emzi0767 for the zlib decompression.
    ///     https://github.com/DSharpPlus/DSharpPlus/blob/master/DSharpPlus/Net/WebSocket/WebSocketClient.cs#L186
    /// </summary>
    internal sealed class WebSocketMessageConverter
    {
        private const ushort _zlibSuffix = 0xFFFF;
        private readonly MemoryStream CompressedStream;
        private MemoryStream DecompressedStream;
        private DeflateStream DecompressionStream;

        public WebSocketMessageConverter()
        {
            CompressedStream = new MemoryStream();
            DecompressedStream = new MemoryStream();
            DecompressionStream = new DeflateStream(CompressedStream, CompressionMode.Decompress);
        }

        public void Reset()
        {
            DecompressionStream.Dispose();
            DecompressionStream = new DeflateStream(CompressedStream, CompressionMode.Decompress);
        }

        public async Task<Payload> DecompressAsync(byte[] buffer)
        {
            if (buffer[0] == 0x78)
                await CompressedStream.WriteAsync(buffer, 2, buffer.Length - 2);
            else
                await CompressedStream.WriteAsync(buffer, 0, buffer.Length);
            CompressedStream.Position = 0;

            if (BitConverter.ToUInt16(buffer, buffer.Length - 2) != _zlibSuffix)
                using (var zlib = new DeflateStream(CompressedStream, CompressionMode.Decompress, true))
                {
                    await zlib.CopyToAsync(DecompressedStream);
                }
            else
                await DecompressionStream.CopyToAsync(DecompressedStream);

            DecompressedStream.Position = 0;
            Payload resultPayload;
            using (var sr = new StreamReader(DecompressedStream, Encoding.UTF8))
            using (var jr = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();

                resultPayload = serializer.Deserialize<Payload>(jr);
            }

            DecompressedStream = new MemoryStream();

            CompressedStream.Position = 0;
            CompressedStream.SetLength(0);

            return resultPayload;
        }
    }
}