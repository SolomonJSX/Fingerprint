namespace Fingerprint.Domain.Entities.Spectrogram;

public struct Band(int min, int max)
{
    public int Min = min;
    public int Max = max;
}