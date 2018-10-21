using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var model = args.Cast<GuildUpdateEventModel>();

            var guildCache = cache[model.Id];

            if (model.IsUnavailable)
            {
                guildCache.IsUnavailable = model.IsUnavailable;
                client.InvokeGuildUnavailable(guildCache.Guild);
            }

            guildCache.Update(guild => 
            {
                guild.OwnerId = model.OwnerId;
                guild.IsUnavailable = model.IsUnavailable;
                guild.Region = model.Region;
                guild.Name = model.Name;
                guild.MemberCount = model.MemberCount;
                guild.Lazy = model.Lazy;
                guild.Large = model.Large;
                guild.AfkTimeout = model.AfkTimeout;
                guild.AfkChannelId = model.AfkChannelId ?? guild.AfkChannelId;
                guild.VerificationLevel = (VerificationLevel)model.VerificationLevel;
                guild.MessageNotificationLevel = (MessageNotificationLevel)model.DefaultMessageNotifications;
                guild.ExplicitContentFilterLevel = (ExplicitContentFilterLevel)model.ExplicitContentFilter;
                guild.RequireMfa = model.RequireMfa;

                foreach (var role in model.Roles)
                    guild.AddOrUpdate(role, old => 
                    {
                        old.Name = role.Name;
                        old.IsHoisted = role.IsHoisted;
                        old.IsManaged = role.IsManaged;
                        old.IsMentionable = role.IsMentionable;
                        old.Permissions = role.Permissions;
                        old.ColorRaw = role.ColorRaw;

                        return old;
                    });
                foreach (var emoji in model.Emojis)
                    guild.AddOrUpdate(emoji, old => 
                    {
                        old.IsAnimated = emoji.IsAnimated;
                        old.Name = emoji.Name;

                        return old;
                    });
            });

            client.InvokeLog(new LogMessage(nameof(GuildUpdateHook), $"Guild: {model.Name} was updated",
                LogSeverity.Verbose));

            client.InvokeGuildUpdated(cache[model.Id].Guild);

            return Task.CompletedTask;
        }
    }
}