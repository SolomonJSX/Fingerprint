using Fingerprint.Alg.Entities;
using Fingerprint.Alg.Entities.Spectrogram;

namespace Fingerprint.Alg.Algorithms;

public class Spectrogram
{
    private const double dspRatio = 4;  
    private const double windowSize = 1024;

    public static double[] Downsample(double[] input, int originalSampleRate, int targetSampleRate)
    {
        if (originalSampleRate <= 0 || targetSampleRate <= 0)
            throw new ArgumentException("Sample rates must be positive");
        
        if (targetSampleRate > originalSampleRate)
            throw new ArgumentException("Target sample rate must be less than or equal to sample rate");
        
        int ratio = originalSampleRate / targetSampleRate;
        
        if (ratio <= 0)
            throw new ArgumentException("Invalid ratio calculated from sample rates.");
        
        var resampled = new List<double>();

        for (var i = 0; i < input.Length; i += ratio)
        {
            int end = i + ratio;
            
            if (end > input.Length)
                end = input.Length;

            double sum = 0.0;
            
            for (int j = i; j < end; j++)
                sum += input[j];

            double avg = sum / (end - i);
            
            resampled.Add(avg);
        }
        
        return resampled.ToArray();
    }
    
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

            var bindBandMaxies = new List<Maxies>();
            
            foreach (var band in bands)
            {
                double maxMag = 0;
                int bestIdx = band.Min;
                
                for (int idx = band.Min; idx < band.Max; idx++)
                {
                    double mag = frame[idx];

                    if (mag > maxMag)
                    {
                        maxMag = mag;
                        bestIdx = idx;
                    }
                }
                
                bindBandMaxies.Add(new Maxies(maxMag = maxMag,  bestIdx = bestIdx));
            }

            foreach (var v in bindBandMaxies)
            {
                maxMags.Add(v.MaxMag);
                freqIndices.Add(v.FreqIdx);
            }

            double maxMagsSum = 0;
            
            foreach (var m in maxMags)
                maxMagsSum += m;

            double average = maxMagsSum / maxMags.Count;

            for (int i = 0; i < maxMags.Count; i++)
            {
                if (maxMags[i] > average)
                {
                    double peakTime = frameIdx * frameDuration;
                    double peakFreq = freqIndices[i] * freqResolution;
                    
                    peaks.Add(new Peak()
                    {
                        Freq =  peakFreq,
                        Time =  peakTime,
                    });
                }
            }
        }

        return peaks;
    }
}