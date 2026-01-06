namespace Fingerprint.API.DTOs;

public class AddSongRequest
{
    public string DownloadUrl { get; set; }
    public string Artist { get; set; }
    public string Title { get; set; }
}