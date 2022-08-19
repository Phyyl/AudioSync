using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Net.Sockets;

namespace AudioSync;

public class AudioSyncClient : IDisposable
{
    private readonly WasapiOut wasapiOut;
    private readonly BufferedWaveProvider waveProvider;
    private readonly TcpClient tcpClient;
    private readonly Task runningTask;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    public AudioSyncClientConfig Config { get; }

    public float Volume
    {
        get => wasapiOut.Volume;
        set => wasapiOut.Volume = Math.Clamp(0, 1, value);
    }

    private AudioSyncClient(TcpClient tcpClient, AudioSyncClientConfig config)
    {
        this.tcpClient = tcpClient;
        Config = config;

        wasapiOut = new(AudioClientShareMode.Shared, config.BufferMs);
        waveProvider = new(wasapiOut.OutputWaveFormat);

        runningTask = StartAsync();
    }

    private async Task StartAsync()
    {
        byte[] buffer = new byte[waveProvider.BufferLength];
        Stream stream = tcpClient.GetStream();

        try
        {
            wasapiOut.Init(waveProvider);
            wasapiOut.Play();

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                int read = await stream.ReadAsync(buffer, cancellationTokenSource.Token);

                waveProvider.AddSamples(buffer, 0, read);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task StopAsync()
    {
        await runningTask;
    }

    public void Dispose()
    {
        wasapiOut.Dispose();
        tcpClient.Dispose();
    }

    public static async Task<AudioSyncClient?> ConnectAsync(AudioSyncClientConfig config)
    {
        try
        {
            TcpClient tcpClient = new()
            {
                NoDelay = true
            };

            await tcpClient.ConnectAsync(config.Hostname, config.Port);

            return new(tcpClient, config);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return default;
    }
}

public record AudioSyncClientConfig(string Hostname, int Port = 4411, int BufferMs = 25);
