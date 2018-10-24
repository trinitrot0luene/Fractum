![AppVeyor](https://img.shields.io/appveyor/ci/trinitrot0luene/Fractum.svg) ![NuGet](https://img.shields.io/nuget/v/Fractum.svg) ![MyGet Pre Release](https://img.shields.io/myget/fractum/vpre/Fractum.svg)

## Links
For a complete API reference, as well as guides and tutorials, check out the latest [documentation](https://trinitrot0luene.github.io/Fractum/).

For pre-release builds, add the MyGet repository as a NuGet package source and check the `Include Prerelease` option (in Visual Studio).

```
https://www.myget.org/F/fractum/api/v3/index.json
```
>Pre-release builds contain untested changes and may behave unexpectedly.

## Usage

Fractum uses a system of Pipelines and PipelineStages to process events coming over the socket. As a developer you can either choose to use the fully complete stages provided out of the box, write your entirely own stages, or mix and match the two. Data is exposed through `EventModels`, allowing you to customise both connection and event handling behaviour, and do additional processing later in the pipeline.

Fractum is currently still under heavy development, however if you're already using it and encounter any problems, just open an issue, shoot me an email: tnt(at)codeforge.pw or contact me on Discord: trinitrotoluene#0001 and tell me about it!

[![](https://discordapp.com/api/guilds/490247210265739274/embed.png?style=banner1&v=1)](https://discord.gg/PCTnd6u)
