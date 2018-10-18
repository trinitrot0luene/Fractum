# Introduction

Fractum is a highly asynchronous library split into two parts to mirror the Discord API. 

Actions that involve updating state such as sending messages, kicking, banning etc. are handled by the REST API, and correspondingly by the `FractumRestClient`. State caching such as which guilds a bot is in, roles, members, and presence information is sent over a WebSocket connection to Discord's Gateway. This is handled by the `FractumSocketClient`, which also exposes events to hook into if you'd like to execute actions in response to them occurring.

The `FractumSocketClient` contains an instantiated `FractumRestClient` through which it makes calls to the REST API, and then populates the returned results with cached data from the Gateway. Therefore unless you have a very specific usecase it is recommmended to never use the `FractumRestClient` on its own if you care about using the returned entities.

For more information about the library, see additional articles in this section as well as the [API Reference](https://trinitrot0luene.github.io/Fractum/api/index.html).