using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildUpdateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var model = args.ToObject<GuildCreateEventModel>();

            var oldGuild = cache.GetGuild(model.Id);

            if (model.IsUnavailable)
            {
                oldGuild.IsUnavailable = model.IsUnavailable;
                client.InvokeGuildUnavailable(oldGuild);
            }

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
                    gc.Roles.AddOrUpdate((a, b) => a.Id == b.Id, role, oldRole =>
                    {
                        oldRole.Name = role.Name;
                        oldRole.IsHoisted = role.IsHoisted;
                        oldRole.IsManaged = role.IsManaged;
                        oldRole.IsMentionable = role.IsMentionable;
                        oldRole.Permissions = role.Permissions;
                        oldRole.ColorRaw = role.ColorRaw;
                    });

                foreach (var emoji in model.Emojis)
                {
                    gc.Emojis.AddOrUpdate((a, b) => a.Id == b.Id, emoji, oldEmoji =>
                    {
                        oldEmoji.IsAnimated = emoji.IsAnimated;
                        oldEmoji.Name = emoji.Name;
                    });
                }
            });

            client.InvokeLog(new LogMessage(nameof(GuildUpdateHook), $"Guild: {model.Name} was updated",
                LogSeverity.Verbose));

            client.InvokeGuildUpdated(cache.GetGuild(model.Id));

            return Task.CompletedTask;
        }
    }
}