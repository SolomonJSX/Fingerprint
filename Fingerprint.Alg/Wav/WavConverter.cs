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
    public static string ConvertToWav(string inputFilePath)
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

        try
        {
            RunFFmpeg(inputFilePath, tmpFile, channels);

            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            File.Move(tmpFile, outputFile);

            return outputFile;
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to convert to WAV. FFmpeg output: {e.Message}", e);
        }
        finally
        {
            if (File.Exists(tmpFile))
                File.Delete(tmpFile);
            
            if (!fileExtension.Equals(".wav", StringComparison.OrdinalIgnoreCase) && File.Exists(inputFilePath))
                File.Delete(inputFilePath);
        }
    }

    public static string ReformatWav(string inputFilePath, int channels)
    {
        if (channels < 1 || channels > 2)
            channels = 1;
        
        string dir  = Path.GetDirectoryName(inputFilePath) ?? string.Empty;
        string fileWithoutExtension = Path.GetFileNameWithoutExtension(inputFilePath);
        
        string outputFile = Path.Combine(dir, fileWithoutExtension + "rfm.wav");
        
        RunFFmpeg(inputFilePath, outputFile, channels);
        
        return outputFile;
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