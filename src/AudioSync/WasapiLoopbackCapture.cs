using NAudio.CoreAudioApi;

public class WasapiLoopbackCapture : WasapiCapture
{
    public WasapiLoopbackCapture(MMDevice? captureDevice = default, bool useEventSync = false, int audioBufferMillisecondsLength = 100)
        : base(captureDevice ?? GetDefaultLoopbackCaptureDevice(), useEventSync, audioBufferMillisecondsLength)
    {
    }

    public static MMDevice GetDefaultLoopbackCaptureDevice()
    {
        return new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
    }

    protected override AudioClientStreamFlags GetAudioClientStreamFlags()
    {
        return AudioClientStreamFlags.Loopback | base.GetAudioClientStreamFlags();
    }
}