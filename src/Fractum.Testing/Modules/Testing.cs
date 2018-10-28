using Fractum.Entities;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

            return Context.RespondAsync("", false, new EmbedBuilder()
                .WithTitle("Pong!")
                .WithDescription($"Up for {upSpan.ToHumanString()}").WithColor(Color.ForestGreen));
        }
    }

    public static class TestingExtensions
    {
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

            return char.ToUpper(result[0]) + result.Substring(1, result.Length - 1);
        }
    }
}
