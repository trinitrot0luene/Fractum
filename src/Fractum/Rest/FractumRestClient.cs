using System.Net.Http;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.Contracts;
using Fractum.Entities.Extensions;
using Fractum.Rest.Utils;
using Fractum.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fractum.Rest
{
    public class FractumRestClient : FractumRestService, IRestClient
    {
        public FractumRestClient(FractumConfig config) : base(config)
        {
        }

        public Task TriggerTypingAsync(ulong channelId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.TYPING));
            return SendRequestAsync(new StringRestRequest(rb, HttpMethod.Post, channelId));
        }

        public async Task<GuildChannel> GetChannelAsync(ulong channelId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS), channelId);
            var resp = await SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get));
            var responseContent = await resp.Content.ReadAsStringAsync();
            var channelType = JObject.Parse(responseContent).Value<ChannelType>("type");
            switch (channelType)
            {
                case ChannelType.GuildText:
                    return JsonConvert.DeserializeObject<TextChannel>(responseContent);
            }

            return null;
        }

        public async Task<Message> CreateMessageAsync(Channel channel, string content, bool isTTS = false,
            EmbedBuilder embedBuilder = null)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channel.Id)
                .WithPart(RouteSection.Create(Consts.MESSAGES));

            var resp = await SendRequestAsync(new StringRestRequest(rb, HttpMethod.Post,
                content: JsonConvert.SerializeObject(new
                {
                    content,
                    tts = isTTS,
                    embed = embedBuilder?.Create()
                })));

            return (await resp.Content.ReadAsStringAsync()).Deserialize<Message>().WithClient(this) as Message;
        }

        public async Task<GatewayBotResponse> GetSocketUrlAsync()
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.GATEWAY))
                .WithPart(RouteSection.Create(Consts.BOT));

            var resp = await SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get));

            return JsonConvert.DeserializeObject<GatewayBotResponse>(await resp.Content.ReadAsStringAsync());
        }
    }
}