# AudioSync

[![NuGet Version](https://img.shields.io/nuget/v/AudioSync.svg?style=flat)](https://www.nuget.org/packages/AudioSync) 

Stream audio from a host to one or multiple clients. Can also be used as a library, see Examples section below.

## Installation

1. Install the dotnet-sdk (6.0.x+)
2. Run `dotnet tool install --global audiosync --version 0.1.1`

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
