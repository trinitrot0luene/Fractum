## About

My first ever serious development project was a Discord bot, making use of the fantastic [Discord.Net](https://github.com/RogueException/Discord.Net) API wrapper. Now that I've had a great time with both other projects and getting to know bot development a better than I ever thought I would, I've decided to come full-circle and write my own wrapper.

## Why use Fractum?

Although in many ways less advanced than other wrappers for the API, Fractum allows you right at the internals, with its Pipeline system giving you the option to interface with raw data coming over the socket, as well as customise caching and connection behaviour to your needs. This is of course optional, with a complete pipeline provided out of the box, so this is just an extra feature for those of you interested in extra customisation. 

Fractum does away with the highly polymorphic nature of Discord.Net, in favour of an approach more similar to DSharpPlus' way of modelling API entities. Out of the box Fractum also comes with a customised version of [ExplosiveCommands](https://github.com/trinitrot0luene/ExplosiveCommands) to give you access to a feature-rich and customisable command framework, along with built-in interactivity.

Fractum is in early stages of development, and as the only developer on both projects I'll do my best to deal with any bugs that arise, as fast as possible. The goal of Fractum is to be both a framework that is simple to "plug-and-play" while at the same time supplying components to those who want to get closer to the "bare metal", so to speak. If either of these- or any point inbetween- sounds appealing to you, feel free to give Fractum a go once I've completed the initial portions of the library.

Fractum is a passion project, but I do also hope to help those out that encounter issues with it, so if you do feel free to shoot me an email: tnt(at)codeforge.pw or contact me on Discord: trinitrotoluene#0001.
