namespace Fingerprint.Core.Utils;

public class Utils
{
    public static uint GenericUniqueId()
    {
        var bytes = new byte[4];
        new Random().NextBytes(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }
    
    public static string GenerateSongKey(string songTitle, string songArtist)
    {
        return $"{songTitle}---{songArtist}";
    }


    public static string GetEnv(string key, string fallback = "")
    {
        var value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrEmpty(value) ? fallback : value;
    }

    public static void ExtendMap<K, V>(Dictionary<K, V> dest, Dictionary<K, V> src)
    {
        foreach (var kvp in src)
        {
            dest[kvp.Key] = kvp.Value;
        }
    }
}