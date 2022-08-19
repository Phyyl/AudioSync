using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace AudioSync;

public class AudioSyncServer : IDisposable
{
    private readonly WasapiLoopbackCapture wasapiLoopbackCapture;
    private readonly TcpListener tcpListener;
    private readonly List<TcpClient> clients = new();
    private readonly Task runningTask;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    public AudioSyncServerConfig Config { get; }

    private AudioSyncServer(TcpListener tcpListener, AudioSyncServerConfig config)
    {
        this.tcpListener = tcpListener;
        Config = config;

        wasapiLoopbackCapture = new(audioBufferMillisecondsLength: config.BufferMs);
        wasapiLoopbackCapture.DataAvailable += WasapiLoopbackCapture_DataAvailable;

        runningTask = StartAsync();
    }

    private void WasapiLoopbackCapture_DataAvailable(object? sender, NAudio.Wave.WaveInEventArgs e)
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
    }

    private async Task StartAsync()
    {
        try
        {
            wasapiLoopbackCapture.StartRecording();

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationTokenSource.Token);
                tcpClient.NoDelay = true;

                clients.Add(tcpClient);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task StopAsync()
    {
        Dispose();

        await runningTask;
    }

    public void Dispose()
{
        cancellationTokenSource.Cancel();

        wasapiLoopbackCapture.Dispose();
        tcpListener.Stop();
    }

    public static AudioSyncServer? Start(AudioSyncServerConfig config)
    {
        try
        {
            TcpListener tcpListener = new(IPAddress.Any, config.Port);
            tcpListener.Start();

            return new AudioSyncServer(tcpListener, config);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return default;
    }
}

public record AudioSyncServerConfig(int Port, int BufferMs = 25);
