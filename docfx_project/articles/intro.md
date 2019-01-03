Fractum provides abstractions for both Discord's REST and Gateway APIs. For further clarification, read below.

# REST

To perform most actions on Discord, clients must send requests through their RESTful HTTP API. This can be done implicitly through helper methods on `Cached` entities when using the `FractumSocketClient`, or explicitly by accessing the `FractumSocketClient#RestClient` or by instantiating your own. Note that if you intend to use the WebSocket and REST API at the same time you should never instantiate new REST clients using the same token, to avoid accidentally hitting the rate limit.

# Gateway

Discord exposes a WebSocket API to dispatch events and updates. When calling `FractumSocketClient#StartAsync()` the client will begin a new session and start to listen for events, as well as populate a cache from received data. You can access this cached data through parameters passed to event handlers as they are called, or through the relevant `IKeyedEnumerable<TEntity>` properties on the client itself.

# Example

```cs
using Fractum;
using Fractum.WebSocket;

namespace MyBot 
{
	public class Program
	{
		private FractumSocketClient _client;
		
		public static CancellationTokenSource _cts = new CancellationTokenSource();
		
		public static Task Main(string args[]) // To enable this signature use language version 7.1+ (<LangVersion>7.1</LangVersion> in your .csproj)
			=> new Program().RunAsync();
			
		public async Task RunAsync()
		{
			_client = new FractumSocketClient(new FractumConfig()
			{
				Token = Environment.GetEnvironmentVariable("bot_token") // Grab token from an environment variable
			});
			
			_client.UseDefaultLogging(LogSeverity.Verbose); // This will register a default logging implementation
			
			_client.OnMessageCreated += async msg => {
				if (msg.Type != MessageType.Default || msg.Author.IsBot) // Only respond to user messages & ignore bots
					return;
				
				if (msg.Content.Equals("!ping"))
					await msg.Channel.CreateMessageAsync("Pong!"); // Use a commands library like Qmmands if you want to implement a command-based bot (Fractum will eventually ship with a default implementation)
			};
			
			await _client.InitialiseAsync(); // Pull information to connect and client information from the API
			
			await _client.StartAsync(); // Start listening for events (non-blocking operation)
			
			await Task.Delay(-1, _cts.Token) // Delay infinitely since otherwise the console application will close
				.ContinueWith(task => _client.StopAsync());
		}
	}
}
```

# Support

Shoot me an email - `tnt(at)codeforge.cc` or hit me up on Discord - `trinitrotoluene#0001`