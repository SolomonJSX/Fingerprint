namespace Fingerprint.Domain.Models;

public class RecordData
{
    public string? Audio { get; set; }
    public double Duration { get; set; }
    public int Channels { get; set; }
    public int SampleRate { get; set; }
    public int SampleSize { get; set; }
}