namespace Fingerprint.Core.Utils;

public static class Helpers
{
    public static void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        else if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    public static void CreateFolder(string folderPath)
    {
        Directory.CreateDirectory(folderPath); 
    }

    public static void ModeFile(string sourcePath, string destinationPath)
    {
        using (var src = File.OpenRead(sourcePath))
        using (var dest = File.Create(destinationPath))
        {
            src.CopyTo(dest);
        }
        
        File.Delete(sourcePath);
    }

    public static byte[] FloatsToBytes(double[] data, int bitsPerSample)
    {
        var bytes = new List<byte>();

        switch (bitsPerSample)
        {
            case 8:
                foreach (var sample in data)
                {
                    // диапазон float должен быть [-1..1]
                    byte val = (byte)((sample + 1.0) * 127.5);
                    bytes.Add(val);
                }
                break;

            case 16:
                foreach (var sample in data)
                {
                    short val = (short)(sample * 32767.0);
                    bytes.AddRange(BitConverter.GetBytes(val)); // little-endian
                }
                break;

            case 24:
                foreach (var sample in data)
                {
                    int val = (int)(sample * 8388607.0);

                    // только младшие 3 байта
                    bytes.Add((byte)(val & 0xFF));
                    bytes.Add((byte)((val >> 8) & 0xFF));
                    bytes.Add((byte)((val >> 16) & 0xFF));
                }
                break;

            case 32:
                foreach (var sample in data)
                {
                    int val = (int)(sample * 2147483647.0);
                    bytes.AddRange(BitConverter.GetBytes(val)); // little-endian
                }
                break;

            default:
                throw new ArgumentException($"Unsupported bitsPerSample: {bitsPerSample}");
        }

        return bytes.ToArray();
    }
}