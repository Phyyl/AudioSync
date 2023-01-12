# AudioSync

[![Build Status](https://img.shields.io/github/actions/workflow/status/phyyl/AudioSync/build.yml?branch=main)](https://github.com/Phyyl/AudioSync/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/AudioSync.svg?style=flat)](https://www.nuget.org/packages/AudioSync)

Stream audio from a host to one or multiple clients. Can also be used as a library, see Examples section below.

## Installation

1. Install the dotnet-sdk (6.0.x+)
2. Run `dotnet tool install --global audiosync --version <version>` (version is required for pre-releases)

## Command Line Usage

Host a server: `audiosync host`

Connect to a server: `audiosync connect <hostname>`

## Examples

Client:

```csharp
AudioSyncClientConfig config = new("hostname", 4411);
if (await AudioSyncClient.ConnectAsync(config) is AudioSyncClient client)
{
    // Wait or do work
    await client.StopAsync();
}
```

Server:
```csharp
AudioSyncServerConfig config = new(4411);
if (AudioSyncServer.Start(config) is AudioSyncServer server)
{
    // Wait or do work
    await server.StopAsync();
}
