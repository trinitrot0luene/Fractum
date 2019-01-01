using Fractum;
using Fractum.Rest;
using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;
using Qmmands;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Fractum.Testing
{
    public sealed class Program
    {
        private FractumSocketClient _client;

        private CommandService _commands = new CommandService(new CommandServiceConfiguration()
        {
            CaseSensitive = true,
            DefaultRunMode = RunMode.Sequential
        });

        static Task Main(string[] args)
            => new Program().RunAsync();

        public async Task RunAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            _client = new FractumSocketClient(new FractumConfig()
            {
                Token = Environment.GetEnvironmentVariable("fractum_token"),
                LargeThreshold = 250,
                MessageCacheLength = 100,
                AlwaysDownloadMembers = false
            });

            _client.OnMessageCreated += HandleMessageCreated;

            _client.OnReady += () => _client.UpdatePresenceAsync("Benchmarking uptime!", ActivityType.Playing, Status.Online);

            _client.UseDefaultLogging(LogSeverity.Verbose);

            await _client.InitialiseAsync();

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
