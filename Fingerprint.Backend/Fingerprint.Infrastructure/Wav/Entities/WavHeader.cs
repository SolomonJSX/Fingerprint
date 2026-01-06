namespace Fingerprint.Core.Wav.Entities;

public class WavHeader
{
    public char[] ChunkID { get; set; }       // "RIFF"
    public int ChunkSize { get; set; }
    public char[] Format { get; set; }        // "WAVE"
    public char[] Subchunk1ID { get; set; }   // "fmt "
    public int Subchunk1Size { get; set; }
    public short AudioFormat { get; set; }   // 1 for PCM
    public short NumChannels { get; set; }
    public int SampleRate { get; set; }
    public int BytesPerSec { get; set; }
    public short BlockAlign { get; set; }
    public short BitsPerSample { get; set; }
    public char[] Subchunk2ID { get; set; }   // "data"
    public int Subchunk2Size { get; set; }
}