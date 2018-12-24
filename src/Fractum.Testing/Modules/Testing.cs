using Fractum;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.Testing.Modules
{
    public class Testing : ModuleBase<CommandContext>
    {
        [Command("ping")]
        public Task PingAsync()
        {
            return Context.RespondAsync("", false, new EmbedBuilder()
                .WithTitle("Pong!")
                .WithField("Socket Uptime", Context.Client.SocketUptime.ToHumanString())
                .WithField("Client Uptime", Context.Client.SessionUptime.ToHumanString())
                .WithColor(Color.Green));
        }

        [Command("cachediagnostics")]
        public Task CacheAsync(ulong userId = 233648473390448641, ulong channelId = 297913371884388353, ulong guildId = 275377268728135680)
        {
            var userSw = Stopwatch.StartNew();
            var user = Context.Client.Users[userId];
            userSw.Stop();

            var channelSw = Stopwatch.StartNew();
            var channel = Context.Client.Channels[channelId];
            channelSw.Stop();

            return Context.RespondAsync("", false, new EmbedBuilder()
                .WithTitle("Cache Diagnostics")
                .WithField("User Count", Context.Client.Users.Count().ToString() + $", time to search = {userSw.Elapsed.TotalMilliseconds}ms")
                .WithField("Channel Count", Context.Client.Channels.Count().ToString()+ $", time to search = {channelSw.Elapsed.TotalMilliseconds}ms")
                .WithField("Guild Count", Context.Client.Guilds.Count().ToString())
                .WithColor(Color.Orange));
        }

        [Command("heapalloc")]
        public Task HeapAllocAsync()
        {
            return Context.RespondAsync($"{Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)}mb");
        }

        [Command("view")]
        public Task ViewAsync(string path)
        {
            return Context.RespondAsync(Context.InspectObject(path));
        }

        [Command("leave")]
        public Task LeaveAsync([Remainder] string name)
            => Context.Client.Guilds.FirstOrDefault(x => x.Name == name).LeaveAsync();
    }

    public static class TestingExtensions
    {
        public static string InspectObject(this object target, string path)
        {
            var pathArray = path.Split('.');
            foreach (var item in pathArray)
            {
                var tempType = target.GetType();
                target = tempType.GetProperty(item, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
            }

            var targetType = target.GetType();
            var targetProps = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var sb = new StringBuilder();
            sb.AppendLine("```ini")
                .AppendLine($"[{targetType.Name}]");

            if (target is IEnumerable<object> targetCollection)
            {
                foreach (var item in targetCollection)
                    sb.AppendLine("  " + item.ToString());
            }
            else
            {
                foreach (var prop in targetProps)
                {
                    var propValue = prop.GetValue(target);
                    var propType = propValue?.GetType();

                    if (propType == null)
                        continue;
                    else
                        sb.AppendLine($"  [{propType.Name}] {prop.Name} : {propValue.ToString()}");
                }
            }

            sb.AppendLine("```");
            return sb.ToString();
        }

        public static string ToHumanString(this TimeSpan timeSpan)
        {
            var dayParts = new[]
            {
                timeSpan.Days == 0 ? null : timeSpan.Days.ToString() + " days",
                timeSpan.Hours == 0 ? null : timeSpan.Hours.ToString() + " hours",
                timeSpan.Minutes == 0 ? null : timeSpan.Minutes.ToString() + " minutes",
                timeSpan.Seconds == 0 ? null : timeSpan.Seconds.ToString() + " seconds"
            }
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();

            var numberOfParts = dayParts.Length;

            string result;

            if (numberOfParts == 1)
                result = dayParts.FirstOrDefault() ?? string.Empty;
            else
                result = string.Join(", ", dayParts, 0, numberOfParts - 1) + " and " + dayParts[numberOfParts - 1];

            return result;
        }
    }
}
