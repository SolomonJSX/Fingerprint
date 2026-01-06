using System.Text;
using Fingerprint.Core.Wav.Entities;

namespace Fingerprint.Infrastructure.Wav;

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

    public static WavInfo ReadWavInfo(string fileName)
    {
        byte[] data = File.ReadAllBytes(fileName);

        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);
        string riff = new string(reader.ReadChars(4));

        int fileSize = reader.ReadInt32();

        string wave = new string(reader.ReadChars(4));

        if (riff != "RIFF" || wave != "WAVE")
            throw new Exception("Это не валидный WAV файл.");
                
        WavHeader header = new WavHeader();

        byte[] audioData = null;

        while (ms.Position < ms.Length)
        {
            if (ms.Position + 8 > ms.Length) break;

            string chunkId = new string(reader.ReadChars(4));
            int chunkSize = reader.ReadInt32();

            if (chunkId == "fmt ")
            {
                header.AudioFormat = reader.ReadInt16();
                header.NumChannels = reader.ReadInt16();
                header.SampleRate = reader.ReadInt32();
                header.BytesPerSec = reader.ReadInt32();
                header.BlockAlign = reader.ReadInt16();
                header.BitsPerSample = reader.ReadInt16();

                if (chunkSize > 16) reader.ReadBytes(chunkSize - 16);
            }
            else if (chunkId == "data")
            {
                audioData = reader.ReadBytes(chunkSize);
                break;
            }
            else
            {
                if (ms.Position + chunkSize <= ms.Length)
                    ms.Position += chunkSize;
                else 
                    break;
            }
        }

        if (audioData == null)
            throw new Exception("В файле не найден чанк с данными (data chunk).");

        var info = new WavInfo()
        {
            Channels = header.NumChannels,
            SampleRate = header.SampleRate,
            Data = audioData
        };
                
        const double scale = 1.0 / 32768.0;
        int sampleCount = audioData.Length / 2;
        short[] int16Buf = new short[sampleCount];
        Buffer.BlockCopy(audioData, 0, int16Buf, 0, audioData.Length);

        if (header.NumChannels == 1)
        {
            info.LeftChannelSamples = int16Buf.Select(s => (double)s * scale).ToArray();
        }
        else if (header.NumChannels == 2)
        {
            int frameCount = sampleCount / 2;
            info.LeftChannelSamples = new double[frameCount];
            info.RightChannelSamples = new double[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                info.LeftChannelSamples[i] = int16Buf[i * 2] * scale;
                info.RightChannelSamples[i] = int16Buf[i * 2 + 1] * scale;
            }
        }
        info.Duration = (double)sampleCount / (header.NumChannels * header.SampleRate);
        return info;
    }


    public static double[] WavBytesToSamples(byte[] input)
    {
        if (input.Length % 2 != 0)
        {
            throw new ArgumentException("Invalid input length");
        }

        int numSamples = input.Length / 2;
        double[] output = new double[numSamples];

        for (int i = 0; i < input.Length; i += 2)
        {
            // Little-endian conversion
            short sample = BitConverter.ToInt16(input, i);
            output[i / 2] = (double)sample / 32768.0;
        }

        return output;
    }
}