using System.Text;

namespace Fingerprint.Alg.Wav;

public static class WavFileUtils
{
    public static void WriteWavFile(string fileName, byte[] data, int sampleRate, int channels, int bitsPerSample)
    {
        if (sampleRate <= 0 || channels <= 0 || bitsPerSample <= 0)
        {
            throw new ArgumentException(
                $"Values must be greater than zero (sampleRate: {sampleRate}, channels: {channels}, bitsPerSample: {bitsPerSample})");
        }

        if (data.Length % channels != 0)
            throw new ArgumentException("Data size not divisible by channels");
        
        using (var fs = new FileStream(fileName, FileMode.Create))
        using (var writer = new BinaryWriter(fs))
        {
            uint subchunk1Size = 16;
            int bytesPerSampleCalc = bitsPerSample / 8;
            ushort blockAlign = (ushort)(bytesPerSampleCalc * channels);
            uint subchunk2Size = (uint)data.Length;
            
            // Запись Header
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write((uint)(36 + data.Length));
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(subchunk1Size);
            writer.Write((ushort)1); // PCM
            writer.Write((ushort)channels);
            writer.Write((uint)sampleRate);
            writer.Write((uint)(sampleRate * channels * bytesPerSampleCalc));
            writer.Write(blockAlign);
            writer.Write((ushort)bitsPerSample);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(subchunk2Size);
            
            writer.Write(data);
        }
    }
}