using System.Diagnostics;

namespace Fingerprint.Alg.Wav;

public static class WavConverter
{
    
    /// <summary>
    /// Конвертирует входной аудиофайл в формат WAV (pcm_s16le, 44100Hz) с заданным количеством каналов.
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static string ConvertToWAV(string inputFilePath)
    {
        if (!File.Exists(inputFilePath))
            throw new FileNotFoundException($"Input file does not exist: {inputFilePath}");

        string toStereoStr = Environment.GetEnvironmentVariable("FINGERPRINT_STEREO") ?? "false";

        if (!bool.TryParse(toStereoStr, out var toStereo))
            throw new ArgumentException(
                $"Failed to convert env variable (FINGERPRINT_STEREO) with value '{toStereoStr}' to bool.");
        
        int channels = toStereo ? 2 : 1;

        string fileExtension = Path.GetExtension(inputFilePath);
        string outputFile = Path.ChangeExtension(inputFilePath, ".wav");

        string dir = Path.GetDirectoryName(outputFile) ?? string.Empty;
        string fileName = Path.GetFileName(outputFile);
        string tmpFile = Path.Combine(dir, "tmp_" + fileName);
        
        
    }

    private static void RunFFmpeg(string input, string output, int channels)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-y -i \"{input}\" -c pcm_s16le -ar 44100 -ac {channels} \"{output}\"",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using (var process = new Process() { StartInfo = processStartInfo })
        {
            process.Start();
            
            string stderr = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Failed to convert to WAV. FFmpeg output: {stderr}");
            }
        }
    }
}