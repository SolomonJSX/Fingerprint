using Fingerprint.Core.Entities.Spectrogram;
using Fingerprint.Core.Wav.Entities;
using Fingerprint.Domain.Entities.Spectrogram;
using Fingerprint.Domain.Models;
using Fingerprint.Infrastructure.Wav;

namespace Fingerprint.Infrastructure.Algorithms
{
    public class Fingerprinter
    {
        private const int MaxFreqBits = 9;
        private const int MaxDeltaBits = 14;
        private const int TargetZoneSize = 5;

        public static Dictionary<int, Couple> Fingerprint(List<Peak> peaks, int songID)
        {
            var fingerprints = new Dictionary<int, Couple>();

            for (int i = 0; i < peaks.Count; i++)
            {
                var anchor = peaks[i];

                for (int j = i + 1; j < peaks.Count && j <= i + TargetZoneSize; j++)
                {
                    var target = peaks[j];

                    int address = CreateAddress(anchor, target);
                    int anchorTimeMs = (int)(anchor.Time * 1000);

                    fingerprints[address] = new Couple()
                    {
                        AnchorTimeMs = anchorTimeMs,
                        SongID = songID
                    };
                }
            }

            return fingerprints;
        }

        private static int CreateAddress(Peak anchor, Peak target)
        {
            int anchorBinFreq = (int)(anchor.Freq / 10);
            int targetBinFreq = (int)(target.Freq / 10);

            int deltaMsRaw = (int)((target.Freq - anchorBinFreq) / 1000);

            int anchorFreqBits = anchorBinFreq & ((1 << MaxFreqBits) - 1);
            int targetFreqBits = targetBinFreq & ((1 << MaxFreqBits) - 1);
            int deltaBits = deltaMsRaw & ((1 << MaxDeltaBits) - 1);

            int address = (anchorFreqBits << 23) | (targetFreqBits << 14) | deltaBits;
            return address;
        }

        public static Dictionary<int, Couple> FingerprintAudio(string songFilePath, int songID)
        {
            // 1. Проверка конвертации
            string wavFilePath = WavConverter.ConvertToWav(songFilePath);
            var fileInfo = new FileInfo(wavFilePath);
            Console.WriteLine($"[Debug] WAV файл создан: {wavFilePath}, размер: {fileInfo.Length} байт");

            if (fileInfo.Length < 1000) 
                throw new Exception("WAV файл подозрительно маленький. Скорее всего, FFmpeg не сконвертировал звук.");

            // 2. Проверка чтения сэмплов
            var wavInfo = WavFileUtils.ReadWavInfo(wavFilePath);
            Console.WriteLine($"[Debug] Прочитано сэмплов: {wavInfo.Data.Length / 2}, Каналов: {wavInfo.Channels}, Частота: {wavInfo.SampleRate}");

            var fingerprint = new Dictionary<int, Couple>();

            // 3. Анализ спектрограммы
            var spectrogram = FingerprintDSP.Spectrogram(wavInfo.LeftChannelSamples, wavInfo.SampleRate);
            Console.WriteLine($"[Debug] Спектрограмма построена: {spectrogram.Count} кадров (frames)");

            if (spectrogram.Count == 0)
                Console.WriteLine("[! Ошибка] Спектрограмма пуста. Возможно, аудио слишком короткое или окно FFT слишком большое.");

            // 4. Анализ пиков
            var peaks = FingerprintDSP.ExtractPeaks(spectrogram.ToArray(), wavInfo.Duration, wavInfo.SampleRate).ToList();
            Console.WriteLine($"[Debug] Извлечено пиков: {peaks.Count}");

            if (peaks.Count < 2)
                Console.WriteLine("[! Ошибка] Недостаточно пиков для создания пар (нужно минимум 2).");

            // 5. Финальное хеширование
            var result = Fingerprint(peaks, songID);
    
            return result;
        }
        
        private static void ProcessChannel(double[] samples, int sampleRate, double duration, int songID, Dictionary<int, Couple> accumulator)
        {
            var spectrogramList = FingerprintDSP.Spectrogram(samples, sampleRate);
            double[][] spectrogramArray = spectrogramList.ToArray();

            var peaksEnumerable = FingerprintDSP.ExtractPeaks(spectrogramArray, duration, sampleRate);
            var peaksList = peaksEnumerable.ToList();

            var channelFingerprints = Fingerprint(peaksList, songID);
            
            foreach (var kvp in channelFingerprints)
            {
                accumulator[kvp.Key] = kvp.Value;
            }
        }
    }
}
