using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities.Extensions;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

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

        private readonly Encoding _utf8 = Encoding.UTF8;
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

        public async Task<IPayload<EventModelBase>> DecompressAsync(byte[] buffer)
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

            var decompressedString = _utf8.GetString(DecompressedStream.ToArray());

            var payload = decompressedString.Deserialize<Payload>();

            DecompressedStream = new MemoryStream();

            CompressedStream.Position = 0;
            CompressedStream.SetLength(0);

            return SelectPayload(payload, decompressedString);
        }

        private IPayload<EventModelBase> SelectPayload(Payload payload, string decompressedString)
        {
            switch (payload.OpCode)
            {
                case OpCode.Hello:
                    return decompressedString.Deserialize<Payload<HelloEventModel>>();
                case OpCode.InvalidSession:
                {
                    var resumable = decompressedString.Deserialize<bool>();
                    var invalidSessionModel = new InvalidSessionEventModel {Resumable = resumable};
                    return new Payload<InvalidSessionEventModel>
                    {
                        OpCode = payload.OpCode,
                        Data = invalidSessionModel
                    };
                }

                #region Dispatches

                case OpCode.Dispatch:
                    switch (payload.Type)
                    {
                        case "BAN_ADD":
                            return decompressedString.Deserialize<Payload<BanAddEventModel>>();
                        case "BAN_REMOVE":
                            return decompressedString.Deserialize<Payload<BanRemoveEventModel>>();
                        case "CHANNEL_CREATE":
                            return decompressedString.Deserialize<Payload<ChannelCreateUpdateOrDeleteEventModel>>();
                        case "CHANNEL_DELETE":
                            return decompressedString.Deserialize<Payload<ChannelCreateUpdateOrDeleteEventModel>>();
                        case "CHANNEL_PINS_UPDATE":
                            return decompressedString.Deserialize<Payload<ChannelPinsUpdateEventModel>>();
                        case "CHANNEL_UPDATE":
                            return decompressedString.Deserialize<Payload<ChannelCreateUpdateOrDeleteEventModel>>();
                        case "EMOJIS_UPDATE":
                            return decompressedString.Deserialize<Payload<EmojisUpdateEventModel>>();
                        case "GUILD_CREATE":
                            return decompressedString.Deserialize<Payload<GuildCreateEventModel>>();
                        case "GUILD_DELETE":
                            return decompressedString.Deserialize<Payload<GuildDeleteEventModel>>();
                        case "GUILD_UPDATE":
                            return decompressedString.Deserialize<Payload<GuildUpdateEventModel>>();
                        case "GUILD_MEMBER_ADD":
                            return decompressedString.Deserialize<Payload<GuildMemberAddEventModel>>();
                        case "GUILD_MEMBER_REMOVE":
                            return decompressedString.Deserialize<Payload<GuildMemberRemoveEventModel>>();
                        case "GUILD_MEMBERS_CHUNK":
                            return decompressedString.Deserialize<Payload<GuildMembersChunkEventModel>>();
                        case "GUILD_MEMBER_UPDATE":
                            return decompressedString.Deserialize<Payload<GuildMemberUpdateEventModel>>();
                        case "INTEGRATIONS_UPDATED":
                            return decompressedString.Deserialize<Payload<IntegrationsUpdatedEventModel>>();
                        case "MESSAGE_CREATE":
                            return decompressedString.Deserialize<Payload<MessageCreateEventModel>>();
                        case "MESSAGE_DELETE":
                            return decompressedString.Deserialize<Payload<MessageDeleteEventModel>>();
                        case "MESSAGE_UPDATE":
                            return decompressedString.Deserialize<Payload<MessageUpdateEventModel>>();
                        case "PRESENCE_UPDATE":
                            return decompressedString.Deserialize<Payload<PresenceUpdateEventModel>>();
                        case "MESSAGE_REACTION_ADD":
                            return decompressedString.Deserialize<Payload<ReactionAddEventModel>>();
                        case "MESSAGE_REACTION_REMOVE":
                            return decompressedString.Deserialize<Payload<ReactionRemoveEventModel>>();
                        case "MESSAGE_REACTION_REMOVE_ALL":
                            return decompressedString.Deserialize<Payload<ReactionsClearEventModel>>();
                        case "GUILD_ROLE_CREATE":
                            return decompressedString.Deserialize<Payload<RoleCreateEventModel>>();
                        case "GUILD_ROLE_DELETE":
                            return decompressedString.Deserialize<Payload<RoleDeleteEventModel>>();
                        case "GUILD_ROLE_UPDATE":
                            return decompressedString.Deserialize<Payload<RoleUpdateEventModel>>();
                        case "USER_UPDATE":
                            return decompressedString.Deserialize<Payload<UserUpdateEventModel>>();
                        case "READY":
                            return decompressedString.Deserialize<Payload<ReadyEventModel>>();
                        default:
                            return decompressedString.Deserialize<Payload<EmptyEventModel>>();
                    }

                #endregion

                default:
                    return new Payload<EmptyEventModel>
                    {
                        OpCode = payload.OpCode,
                        Data = new EmptyEventModel()
                    };
            }
        }
    }
}