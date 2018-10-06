using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.WebSocket.Core
{
    /// <summary>
    ///     Decompresses Gateway messages to plaintext.
    ///     Credit to Emzi0767 for this.
    ///     https://github.com/DSharpPlus/DSharpPlus/blob/master/DSharpPlus/Net/WebSocket/WebSocketClient.cs#L186
    /// </summary>
    internal sealed class WebSocketMessageConverter
    {
        private const ushort _zlibSuffix = 0xFFFF;
        private readonly MemoryStream CompressedStream;
        private readonly MemoryStream DecompressedStream;
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

        public async Task<string> DecompressAsync(byte[] buffer)
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

            buffer = DecompressedStream.ToArray();

            DecompressedStream.Position = 0;
            DecompressedStream.SetLength(0);
            CompressedStream.Position = 0;
            CompressedStream.SetLength(0);

            return Encoding.UTF8.GetString(buffer);
        }
    }
}