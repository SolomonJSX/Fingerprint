using Fingerprint.Domain.Entities;
using Fingerprint.Domain.Models;
using Fingerprint.Infrastructure.Services;
using Fingerprint.Infrastructure.Services.Implementations;
using Fingerprint.Infrastructure.Services.Interfaces;

namespace Fingerprint.Core.Services;

public class RecognitionService(IStorageService storageService)
{
    public RecognitionResult Match(Dictionary<int, Couple> queryFingerprints)
    {
        var queryHashes = queryFingerprints.Keys.ToArray();
        Console.WriteLine($"[Debug] Ищем в базе совпадения для {queryHashes.Length} хешей...");

        var matches = storageService.FindMatches(queryHashes);

        if (matches == null || !matches.Any())
        {
            Console.WriteLine("[! Ошибка] База данных не вернула ни одного совпадения для этих хешей.");
            return null;
        }

        Console.WriteLine($"[Debug] Всего найдено {matches.Count} сырых совпадений в БД.");

        var groupedResults = matches
            .Select(m => new
            {
                m.SongId, m.Artist, m.Title,
                // Квантуем сдвиг: делим на 50, чтобы объединить близкие совпадения
                // 45289 / 50 = 905
                // 45300 / 50 = 906 (они окажутся в соседних корзинах или в одной)
                // Лучше использовать деление на 100 для записи с микрофона
                RawOffset = m.DbTime - (int)queryFingerprints[(int)m.Hash].AnchorTimeMs
            })
            .GroupBy(x => new { x.SongId, x.Artist, x.Title, BinnedOffset = x.RawOffset / 100 }) 
            .Select(g => new
            {
                g.Key.SongId, g.Key.Artist, g.Key.Title,
                Count = g.Count(),
                MaxOffset = g.Max(x => x.RawOffset) // Для логов
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        // Выводим топ-3 кандидата, даже если у них 1-2 совпадения
        foreach (var cand in groupedResults.Take(3))
        {
            Console.WriteLine($"[Debug] Кандидат: {cand.Artist} - {cand.Title}, Совпадений: {cand.Count}, Сдвиг: {cand.MaxOffset}");
        }

        var bestResult = groupedResults.FirstOrDefault();

        // Если совпадений меньше 5 (для теста), считаем что не нашли
        if (bestResult == null || bestResult.Count < 5) 
        {
            Console.WriteLine("[Debug] Лучший результат не прошел порог уверенности (нужно хотя бы 5).");
            return null;
        }
    
        return new RecognitionResult
        {
            SongId = bestResult.SongId,
            Artist = bestResult.Artist,
            Title = bestResult.Title,
            Score = bestResult.Count
        };
    }
}