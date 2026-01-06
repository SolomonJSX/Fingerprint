namespace Fingerprint.Core.Wav.Entities;

public class WavInfo
{
    public int Channels { get; set; }
    public int SampleRate { get; set; }
    public double Duration { get; set; }
    public byte[] Data { get; set; }
    public double[] LeftChannelSamples { get; set; }
    public double[] RightChannelSamples { get; set; }
}