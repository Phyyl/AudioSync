using AudioSync;
using System.CommandLine;

Argument<string> hostArgument = new("host");
Option<int> portOption = new("--port", () => 4411);

Command hostCommand = new("host");
hostCommand.Add(portOption);
hostCommand.SetHandler(HostAsync, portOption);

Command connectCommand = new("connect");
connectCommand.Add(hostArgument);
connectCommand.Add(portOption);
connectCommand.SetHandler(ConnectAsync, hostArgument, portOption);

RootCommand rootCommand = new();
rootCommand.Add(hostCommand);
rootCommand.Add(connectCommand);
rootCommand.Invoke(args);

async Task ConnectAsync(string hostname, int port)
{
    if (await AudioSyncClient.ConnectAsync(new(hostname, port)) is AudioSyncClient client)
    {
        Console.ReadLine();
        await client.StopAsync();
    }
}

async Task HostAsync(int port)
{
    if (AudioSyncServer.Start(new(port)) is AudioSyncServer server)
    {
        Console.ReadLine();
        await server.StopAsync();
    }
}
