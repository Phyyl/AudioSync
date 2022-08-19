using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.CommandLine;
using System.Net;
using System.Net.Sockets;

Argument<string> hostArgument = new("host");
Option<int> bufferOption = new("--buffer");
Option<int> portOption = new("--port", () => 4411);

Command hostCommand = new("host");
hostCommand.Add(portOption);
hostCommand.Add(bufferOption);
hostCommand.SetHandler(Host, portOption, bufferOption);

Command connectCommand = new("connect");
connectCommand.Add(hostArgument);
connectCommand.Add(portOption);
connectCommand.Add(bufferOption);
connectCommand.SetHandler(Connect, hostArgument, portOption, bufferOption);

RootCommand rootCommand = new();
rootCommand.Add(hostCommand);
rootCommand.Add(connectCommand);
rootCommand.Invoke(args);

void Connect(string hostname, int port, int bufferMs)
{
    using WasapiOut output = new(AudioClientShareMode.Shared, bufferMs);
    BufferedWaveProvider provider = new(output.OutputWaveFormat);
    using TcpClient client = new(hostname, port);
    using Stream stream = client.GetStream();
    byte[] buffer = new byte[1024 * 32];

    client.NoDelay = true;

    output.Init(provider);
    output.Play();

    while (!Console.KeyAvailable)
    {
        try
        {
            int read = stream.Read(buffer, 0, buffer.Length);
            provider.AddSamples(buffer, 0, read);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            break;
        }
    }
}

void Host(int port, int bufferMs)
{
    using WasapiLoopbackCapture capture = new(audioBufferMillisecondsLength: bufferMs);
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
