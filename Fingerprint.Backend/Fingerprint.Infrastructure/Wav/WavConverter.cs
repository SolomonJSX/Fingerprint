using System.Diagnostics;

namespace Fingerprint.Infrastructure.Wav;

public static class WavConverter
{
    private const string OutputFolderName = "converted_wav";
    
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
           throw new FileNotFoundException("File not found", inputFilePath);
       
       string outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OutputFolderName);

       if (!Directory.Exists(outputDirectory))
       {
           Directory.CreateDirectory(outputDirectory);
       }
       
       string toStereoStr = Environment.GetEnvironmentVariable("FINGERPRINT_STEREO") ?? "false";

       bool.TryParse(toStereoStr, out var toStereo);
       
       int channels = toStereo ? 2 : 1;
       
       string fileNameOnly =  Path.GetFileNameWithoutExtension(inputFilePath);
       string outputFile = Path.Combine(outputDirectory, fileNameOnly + ".wav");
       
       string tmpFile = Path.Combine(outputDirectory, "tmp_" + fileNameOnly + ".wav");

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
           throw new Exception($"Failed to convert to WAV: {e.Message}", e);
       }
       finally
       {
           if (File.Exists(tmpFile))
               File.Delete(tmpFile);
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
            RedirectStandardError = true,
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