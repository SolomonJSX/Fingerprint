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
    
            // Рекомендуемое значение TargetZoneSize = 5
            // Это и есть наш Fan-out (один якорь связывается с 5 следующими пиками)
            int targetZoneSize = 5; 

            for (int i = 0; i < peaks.Count; i++)
            {
                var anchor = peaks[i];

                // Формируем пары в целевой зоне
                for (int j = i + 1; j < peaks.Count && j <= i + targetZoneSize; j++)
                {
                    var target = peaks[j];

                    // ВАЖНО: CreateAddress теперь принимает разницу во времени
                    int address = CreateAddress(anchor, target);
            
                    int anchorTimeMs = (int)(anchor.Time * 1000);

                    // Используем TryAdd, чтобы не падать, если вдруг хеши совпали
                    fingerprints.TryAdd(address, new Couple()
                    {
                        AnchorTimeMs = anchorTimeMs,
                        SongID = songID
                    });
                }
            }

            return fingerprints;
        }

        private static int CreateAddress(Peak anchor, Peak target)
        {
            // 1. Квантуем частоты (делим на 10, чтобы небольшие погрешности не меняли хеш)
            int f1 = (int)(anchor.Freq / 10);
            int f2 = (int)(target.Freq / 10);

            // 2. ВЫЧИСЛЯЕМ РЕАЛЬНУЮ РАЗНИЦУ ВО ВРЕМЕНИ (Delta Time)
            // Именно это делает отпечаток уникальным!
            int dt = (int)((target.Time - anchor.Time) * 1000);

            // 3. Ограничиваем значения битовыми масками (чтобы влезть в 32 бита)
            // f1: 9 бит (до 512), f2: 9 бит (до 512), dt: 14 бит (до 16384 мс)
            int f1Bits = f1 & 0x1FF;
            int f2Bits = f2 & 0x1FF;
            int dtBits = dt & 0x3FFF;

            // Сборка хеша:
            // [ f1 (9 бит) | f2 (9 бит) | dt (14 бит) ]
            int address = (f1Bits << 23) | (f2Bits << 14) | dtBits;
    
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
