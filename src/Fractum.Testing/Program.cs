using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket;
using Qmmands;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Fractum.Testing
{
    public sealed class Program
    {
        private FractumSocketClient _client;

        private CommandService _commands;

        public static DateTimeOffset StartedAt;

        static Task Main(string[] args)
            => new Program().RunAsync();

        public async Task RunAsync()
        {
            _client = new FractumSocketClient(new FractumConfig()
            {
                Token = Environment.GetEnvironmentVariable("fractum_token"),
                LargeThreshold = 250,
                MessageCacheLength = 100,
                AlwaysDownloadMembers = false
            });

            _commands = new CommandService(new CommandServiceConfiguration()
            {
                CaseSensitive = true,
                DefaultRunMode = RunMode.Sequential
            });

            _client.GetPipeline()
                .AddConnectionStage()
                .AddEventStage();

            _client.UseDefaultLogging(LogSeverity.Info, true);

            _client.MessageCreated += HandleMessageCreated;

            _client.Ready += async () =>
            {
                await _client.UpdatePresenceAsync("Uptime benchmarking", ActivityType.Playing, Status.Idle);
            };

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            await _client.GetConnectionInfoAsync();

            StartedAt = DateTimeOffset.UtcNow;

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task HandleMessageCreated(CachedMessage message)
        {
            if (!message.IsUserMessage || message.Author.IsBot)
                return;

            if (CommandUtilities.HasPrefix(message.Content, '>', false, out var commandString))
            {
                var result = await _commands.ExecuteAsync(commandString, new CommandContext(_client, message));
            }
        }
    }
}
