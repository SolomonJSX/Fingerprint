using Fingerprint.Alg.Entities;
using Fingerprint.Alg.Entities.Spectrogram;

namespace Fingerprint.Alg.Algorithms;

public class Spectrogram
{
    private const int dspRatio = 4;  
    
    public IEnumerable<Peak> ExtractPeaks(double[,] spectrogram, double audioDuration, double sampleRate)
    {
        if (spectrogram.Length < 1)
            return [];
        
        Band[] bands =
        [
            new Band(0, 10),
            new Band(10, 20),
            new Band(20, 40),
            new Band(40, 80),
            new Band(80, 160),
            new Band(160, 512)
        ];

        var peaks = new List<Peak>();

        var frameDuration = audioDuration / (double) spectrogram.Length;
        
        double effectiveSampleRate = sampleRate / (double) dspRatio;
        

        return default;
    }
}