namespace Fingerprint.Domain.Entities;

public class RecognitionResult
{
    public int SongId { get; set; }
    public string Artist { get; set; }
    public string Title { get; set; }
    public int Score { get; set; } // Количество совпавших хешей с одним сдвигом
}