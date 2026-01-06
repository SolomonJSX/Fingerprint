namespace Fingerprint.Domain.Entities.Spectrogram;

public struct Maxies(double maxMag, int freqIdx)
{
    public double MaxMag { get; set; } = maxMag;
    public int FreqIdx { get; set; } = freqIdx;
}