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
        var matches = storageService.FindMatches(queryFingerprints.Keys.ToArray());

        if (!matches.Any()) return null;

        var results = matches
            .Select(m => new
            {
                m.SongId,
                m.Artist,
                m.Title,
                Offset = m.DbTime - (int)queryFingerprints[(int)m.Hash].AnchorTimeMs
            })
            .GroupBy(x => new { x.SongId, x.Artist, x.Title, x.Offset })
            .Select(g => new
            {
                g.Key.SongId,
                g.Key.Artist,
                g.Key.Title,
                Count = g.Count(),
            })
            .OrderByDescending(s => s.Count)
            .FirstOrDefault();

        if (results == null || results.Count < 10) return null;
        
        return new RecognitionResult
        {
            SongId = results.SongId,
            Artist = results.Artist,
            Title = results.Title,
            Score = results.Count
        };
    }
}