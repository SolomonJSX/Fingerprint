using Fingerprint.API.DTOs;
using Fingerprint.Infrastructure.Algorithms;
using Fingerprint.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fingerprint.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecognitionController(IStorageService storageService, HttpClient httpClient) : ControllerBase
{
    [HttpPost("add-song")]
    public async Task<ActionResult> AddSong(AddSongRequest request)
    {
        if (string.IsNullOrEmpty(request.DownloadUrl))
            return BadRequest("URL не может быть пустым");

        string temptPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp3");

        try
        {
            var response = await httpClient.GetAsync(request.DownloadUrl);

            if (!response.IsSuccessStatusCode)
                return BadRequest("Не удалось скачать файл по указанной ссылке");

            {
                await using var stream = new FileStream(temptPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(stream);
                await stream.FlushAsync();
            }

            int songId = storageService.SaveSong(request.Artist, request.Title);

            var fingerprints = await Task.Run(() => Fingerprinter.FingerprintAudio(temptPath, songId));

            // 5. Сохраняем тысячи отпечатков в таблицу Fingerprints
            storageService.SaveFingerprints(fingerprints, songId);

            return Ok(new
            {
                Message = "Песня успешно добавлена и проиндексирована",
                SongId = songId,
                FingerprintsCount = fingerprints.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
        }
        finally
        {
            if (System.IO.File.Exists(temptPath))
                System.IO.File.Delete(temptPath);
        }
    }
}