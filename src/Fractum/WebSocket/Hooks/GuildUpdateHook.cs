using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    public class GuildUpdateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var model = args.ToObject<GuildCreateEventModel>();

            var oldGuild = cache.GetGuild(model.Id);

            cache.UpdateGuildCache(model.Id, gc =>
            {
                gc.UpdateGuild(guild =>
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
                    guild.VerificationLevel = (VerificationLevel) model.VerificationLevel;
                    guild.MessageNotificationLevel = (MessageNotificationLevel) model.DefaultMessageNotifications;
                    guild.ExplicitContentFilterLevel = (ExplicitContentFilterLevel) model.ExplicitContentFilter;
                    guild.RequireMfa = model.RequireMfa;
                });
                foreach (var role in model.Roles)
                    gc.Roles.AddOrUpdate(role.Id, role, (k, oldRole) =>
                    {
                        oldRole.Name = role.Name;
                        oldRole.IsHoisted = role.IsHoisted;
                        oldRole.IsManaged = role.IsManaged;
                        oldRole.IsMentionable = role.IsMentionable;
                        oldRole.Permissions = role.Permissions;
                        oldRole.ColorRaw = role.ColorRaw;
                        return oldRole;
                    });

                foreach (var emoji in model.Emojis)
                    gc.Emojis.AddOrUpdate(emoji.Id, emoji, (k, oldEmoji) =>
                    {
                        oldEmoji.IsAnimated = emoji.IsAnimated;
                        oldEmoji.Name = emoji.Name;

                        return oldEmoji;
                    });
            });

            client.InvokeLog(new LogMessage(nameof(GuildUpdateHook), $"Guild: {model.Name} was updated",
                LogSeverity.Verbose));

            client.InvokeGuildUpdated(cache.GetGuild(model.Id));

            return Task.CompletedTask;
        }
    }
}