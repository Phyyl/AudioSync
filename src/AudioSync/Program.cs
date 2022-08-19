using AudioSync;
using System.CommandLine;
using System.Net;
using System.Net.Sockets;

Argument<string> hostArgument = new("host");
Option<int> portOption = new("--port", () => 4411);

Command hostCommand = new("host");
hostCommand.Add(portOption);
hostCommand.SetHandler(Host, portOption);

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

void Host(int port)
{
    using WasapiLoopbackCapture capture = new(audioBufferMillisecondsLength: 25);
    List<TcpClient> clients = new();
    TcpListener listener = new(IPAddress.Any, port);

    capture.DataAvailable += (sender, e) =>
    {
        foreach (var client in clients.ToArray())
        {
            try
            {
                client.GetStream().Write(e.Buffer, 0, e.BytesRecorded);
            }
            catch
            {
                clients.Remove(client);
            }
        }
    };

    capture.StartRecording();
    listener.Start();

    while (!Console.KeyAvailable)
    {
        TcpClient client = listener.AcceptTcpClient();
        client.NoDelay = true;
        clients.Add(client);
    }
}
