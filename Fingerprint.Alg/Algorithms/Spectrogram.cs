using Fingerprint.Alg.Entities;
using Fingerprint.Alg.Entities.Spectrogram;

namespace Fingerprint.Alg.Algorithms;

public class Spectrogram
{
    private const double dspRatio = 4;  
    private const double windowSize = 1024;
    
    public IEnumerable<Peak> ExtractPeaks(double[][] spectrogram, double audioDuration, double sampleRate)
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
        double freqResolution = effectiveSampleRate / windowSize;

        for (int frameIdx = 0; frameIdx < spectrogram.Length; frameIdx++)
        {
            var frame = spectrogram[frameIdx];

            List<double> maxMags = new();
            List<int> freqIndices = new();

            List<Maxies> binBandMaxies = new();

            foreach (var band in bands)
            {
                Maxies maxx = new Maxies(0, band.Min);
                double maxMag = 0;

                for (int idx = band.Min; idx <= band.Max; idx++)
                {
                    double mag = frame[idx];

                    if (mag > maxMag)
                    {
                        maxMag = mag;
                        maxx = new Maxies(maxMag, idx);
                    }
                }
                
                binBandMaxies.Add(maxx);
            }
        }

        return default;
    }
}