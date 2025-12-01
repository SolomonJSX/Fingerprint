namespace Fingerprint.Alg.Wav.Entities;

public class WavHeader
{
    public char[] ChunkID { get; set; }       // "RIFF"
    public uint ChunkSize { get; set; }
    public char[] Format { get; set; }        // "WAVE"
    public char[] Subchunk1ID { get; set; }   // "fmt "
    public uint Subchunk1Size { get; set; }
    public ushort AudioFormat { get; set; }   // 1 for PCM
    public ushort NumChannels { get; set; }
    public uint SampleRate { get; set; }
    public uint BytesPerSec { get; set; }
    public ushort BlockAlign { get; set; }
    public ushort BitsPerSample { get; set; }
    public char[] Subchunk2ID { get; set; }   // "data"
    public uint Subchunk2Size { get; set; }
}