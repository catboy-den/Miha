# Miha, a discord bot for Midnight Haven

- .NET 8 based, Redis backed
- [Discord.NET based](https://github.com/discord-net/Discord.Net)
- Built & structured around dependency injection
- Rapidly integrated using GitHub Actions
- Rapidly deployed in a k8s cluster using flux image automation

## The idea & motivations

- Assist in the QoL around the community, by announcing events when they start, providing an event schedule for the week, or keeping track of birthdays in the community as a few examples
- Take advantage of the most recent modernizations of the .NET platform, allowing for Discord <-> VRChat possibilities, say for example event schedules in the Midnight Havens' home world, or even birthdays by utilizing Udons' most recent improvements in networking alloing JSON-esque calls
- Easily maintainable and fairly documented, written in a language that's very easy to learn, update, and used even within Unity for VRChat

## Hosting it yourself

- Miha is compiled for you already as a docker image

## Development notes

### Solution structure

- `Miha`
- `Miha.Discord`
- `Miha.Logic`
- `Miha.Redis`
- `Miha.Shared`
