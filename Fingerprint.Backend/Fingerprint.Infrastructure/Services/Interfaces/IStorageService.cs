using System.Data;
using Fingerprint.Domain.Models;
using Fingerprint.Infrastructure.Entities;

namespace Fingerprint.Infrastructure.Services.Interfaces;

public interface IStorageService
{
    int SaveSong(string artist, string title);
    void SaveFingerprints(Dictionary<int, Couple> fingerprints, int songId);
    List<MatchResult> FindMatches(int[] hashes);
}