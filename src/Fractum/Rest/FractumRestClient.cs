using System.Net.Http;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.Contracts;
using Fractum.Entities.Extensions;
using Fractum.Rest.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fractum.Rest
{
    public sealed class FractumRestClient : FractumRestService, IRestClient
    {
        public FractumRestClient(FractumRestConfig config) : base(config)
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

        internal async Task<Message> CreateMessageAsync(Channel channel, string content, bool isTTS = false,
            EmbedBuilder EmbedBuilder = null)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channel.Id)
                .WithPart(RouteSection.Create(Consts.MESSAGES));

            var resp = await SendRequestAsync(new StringRestRequest(rb, HttpMethod.Post,
                content: JsonConvert.SerializeObject(new
                {
                    content,
                    tts = isTTS,
                    embed = EmbedBuilder?.Create()
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