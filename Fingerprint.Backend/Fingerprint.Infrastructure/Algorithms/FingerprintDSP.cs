using Fingerprint.Core.Entities.Spectrogram.Enums;
using Fingerprint.Domain.Entities.Spectrogram;

namespace Fingerprint.Infrastructure.Algorithms;

public class FingerprintDSP
{
    const int dspRatio = 4;
    const int windowSize = 1024;
    const double maxFreq = 5000.0;
    const int hopSize = windowSize / 2;
    const WindowType windowType = WindowType.Hanning;
    

    public static List<double[]> Spectrogram(double[] sample, int sampleRate)
    {
        var filtered = LowPassFilter(maxFreq, sampleRate, sample);
        
        var downSampled = Downsample(filtered, sampleRate, sampleRate / dspRatio);
        
        var window = new double[windowSize];

        for (int i = 0; i < window.Length; i++)
        {
            double theta = 2 * Math.PI * i / (windowSize - 1);
            if (windowType == WindowType.Hamming) 
                window[i] = 0.54 - 0.46 * Math.Cos(theta);
            else
                window[i] = 0.5 - 0.5 * Math.Cos(theta);
        }
        
        var spectrogram = new List<double[]>();

        for (int start = 0; start + windowSize <= downSampled.Length; start += hopSize)
        {
            int end = start + windowSize;
            
            var frame = new double[windowSize];
            
            Array.Copy(downSampled, start, frame, 0, windowSize);
            
            for (int j = 0; j < windowSize; j++)
                frame[j] *= window[j];
            
            var fftResult = FFTUtil.FFT(frame);
            
            var mag = new double[fftResult.Length / 2];

            for (int j = 0; j < mag.Length; j++)
                mag[j] = fftResult[j].Magnitude;
            
            spectrogram.Add(mag);
        }
        
        return spectrogram;
    }

    public  static double[] LowPassFilter(double cutoffFrequency, double sampleRate, double[] input)
    {
        double rc = 1.0 / (2 * Math.PI * cutoffFrequency);
        double dt = 1.0 / sampleRate;
        double alpha = dt / (rc + dt);
        
        double[] filteredSignal = new double[input.Length];

        double prevOutput = 0.0;
        
        for (int i = 0; i < input.Length; i++)
        {
            var x = input[i];

            if (i == 0)
            {
                filteredSignal[i] = x * alpha;
            }
            else
            {
                filteredSignal[i] = alpha * x + (1 - alpha) * prevOutput;
            }
            
            prevOutput = filteredSignal[i];
        }
        
        return filteredSignal;
    }
    
    public static double[] Downsample(double[] input, int originalSampleRate, int targetSampleRate)
    {
        if (originalSampleRate <= 0 || targetSampleRate <= 0)
            throw new Exception("Sample rates must be positive");
        
        if (targetSampleRate > originalSampleRate)
            throw new Exception("Target sample rate must be less than or equal to sample rate");
        
        int ratio = originalSampleRate / targetSampleRate;
        
        if (ratio <= 0)
            throw new Exception("Invalid ratio calculated from sample rates.");
        
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
    
    public static IEnumerable<Peak> ExtractPeaks(double[][] spectrogram, double audioDuration, double sampleRate)
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