using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventModel = (GuildUpdateEventModel) args;

            var guildCache = cache[eventModel.Id];

            if (eventModel.IsUnavailable)
            {
                guildCache.IsUnavailable = eventModel.IsUnavailable;
                client.InvokeGuildUnavailable(guildCache.Guild);
            }

            guildCache.Update(guild =>
            {
                guild.OwnerId = eventModel.OwnerId;
                guild.IsUnavailable = eventModel.IsUnavailable;
                guild.Region = eventModel.Region;
                guild.Name = eventModel.Name;
                guild.MemberCount = eventModel.MemberCount;
                guild.Lazy = eventModel.Lazy;
                guild.Large = eventModel.Large;
                guild.AfkTimeout = eventModel.AfkTimeout;
                guild.AfkChannelId = eventModel.AfkChannelId ?? guild.AfkChannelId;
                guild.VerificationLevel = (VerificationLevel) eventModel.VerificationLevel;
                guild.MessageNotificationLevel = (MessageNotificationLevel) eventModel.DefaultMessageNotifications;
                guild.ExplicitContentFilterLevel = (ExplicitContentFilterLevel) eventModel.ExplicitContentFilter;
                guild.RequireMfa = eventModel.RequireMfa;

                guild.Replace(eventModel.Roles);
                guild.Replace(eventModel.Emojis);
            });

            client.InvokeLog(new LogMessage(nameof(GuildUpdateHook), $"Guild: {eventModel.Name} was updated",
                LogSeverity.Verbose));

            client.InvokeGuildUpdated(cache[eventModel.Id].Guild);

            return Task.CompletedTask;
        }
    }
}