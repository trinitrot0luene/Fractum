using Fractum.Entities;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.Testing.Modules
{
    public class Testing : ModuleBase<CommandContext>
    {
        [Command("ping")]
        public Task PingAsync()
        {
            var upSpan = DateTimeOffset.UtcNow - Program.StartedAt;

            return Context.RespondAsync("", false, new EmbedBuilder().WithTitle("Pong!").WithDescription($"Up for {upSpan.ToHumanTimeString()}").WithColor(Color.ForestGreen));
        }
    }

    public static class TestingExtensions
    {
        public static string ToHumanTimeString(this TimeSpan span, int significantDigits = 3)
        {
            var format = "G" + significantDigits;
            return span.TotalMilliseconds < 1000 ? span.TotalMilliseconds.ToString(format) + " milliseconds"
                : (span.TotalSeconds < 60 ? span.TotalSeconds.ToString(format) + " seconds"
                    : (span.TotalMinutes < 60 ? span.TotalMinutes.ToString(format) + " minutes"
                        : (span.TotalHours < 24 ? span.TotalHours.ToString(format) + " hours"
                                                : span.TotalDays.ToString(format) + " days")));
        }
    }
}
