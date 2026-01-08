using Fingerprint.API.DTOs;
using Fingerprint.Core.Services;
using Fingerprint.Infrastructure.Algorithms;
using Fingerprint.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fingerprint.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecognitionController(IStorageService storageService, HttpClient httpClient, RecognitionService recognitionService) : ControllerBase
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

    [HttpPost("identify")]
    public async Task<ActionResult> Identify(IFormFile audioFile)
    {
        if (audioFile == null || audioFile.Length == 0)
            return BadRequest("Аудиофайл не получен");

        string tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");

        try
        {
            // 1. Сохраняем присланный отрывок во временный файл
            await using var stream = new FileStream(tempPath, FileMode.Create);
            await audioFile.CopyToAsync(stream);

            var queryFingerprints = await Task.Run(() => Fingerprinter.FingerprintAudio(tempPath, 0));

            var result = recognitionService.Match(queryFingerprints);
            
            if (result == null)
            {
                return NotFound(new { Message = "Песня не найдена в базе данных" });
            }

            return Ok(result); // Возвращает Artist, Title и Score
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при распознавании: {ex.Message}");
        }
    }
}