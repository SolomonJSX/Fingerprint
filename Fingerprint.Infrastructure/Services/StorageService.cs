using System.Data;
using Dapper;
using Fingerprint.Domain.Models;
using Fingerprint.Infrastructure.Entities;
using Npgsql;

namespace Fingerprint.Infrastructure.Services;

public class StorageService(string connectionString)
{
    private IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(connectionString);
        // Для SQL Server: return new SqlConnection(_connectionString);
    }

    /// <summary>
    /// Сохраняет песню в таблицу Songs и возвращает её ID.
    /// </summary>
    public int SaveSong(string artist, string title)
    {
        using (var db = CreateConnection())
        {
            // Вставка и возврат сгенерированного ID
            // Синтаксис RETURNING Id специфичен для PostgreSQL.
            // Для MSSQL используйте: "INSERT INTO ... VALUES ...; SELECT CAST(SCOPE_IDENTITY() as int)"
            string sql = "INSERT INTO Songs (Artist, Title) VALUES (@Artist, @Title) RETURNING Id;";
            return db.QuerySingle<int>(sql, new { Artist = artist, Title = title });
        }
    }

    /// <summary>
    /// Сохраняет массив отпечатков в таблицу Fingerprints.
    /// </summary>
    /// <param name="fingerprints"></param>
    /// <param name="songId"></param>
    public void SaveFingerprints(Dictionary<int, Couple> fingerprints, int songId)
    {
        if (fingerprints == null || fingerprints.Count == 0) 
        {
            Console.WriteLine("Предупреждение: Словарь отпечатков пуст. Нечего сохранять.");
            return;
        }
        using (var db = CreateConnection())
        {
            db.Open();

            using (var transaction = db.BeginTransaction())
            {
                var batch = fingerprints.Select(kvp => new FingerprintDTO()
                {
                    Hash = kvp.Key,
                    SongId = songId,
                    TimeAnchor = kvp.Value.AnchorTimeMs
                }).ToList();

                string sql =
                    "INSERT INTO fingerprints (SongId, Hash, TimeAnchor)  VALUES (@SongId, @Hash, @TimeAnchor)";

                Console.WriteLine($"Начинаю вставку {batch.Count} записей в БД...");
                
                int affectedRows = db.Execute(sql, batch, transaction: transaction);

                transaction.Commit();
                
                Console.WriteLine($"Успешно зафиксировано строк в БД: {affectedRows}");
            }
        }
    }

    public IEnumerable<MatchResult> FindMatches(IEnumerable<int> hashes)
    {
        using (var db = CreateConnection())
        {
            // ANY(@Hashes) - синтаксис Postgres для поиска в массиве. 
            // Для MSSQL нужно использовать IN (...)
            string sql = @"
                    SELECT 
                        s.Id as SongId, 
                        s.Title, 
                        s.Artist, 
                        f.TimeAnchor as DbTime, 
                        f.Hash 
                    FROM Fingerprints f
                    JOIN Songs s ON f.SongId = s.Id
                    WHERE f.Hash = ANY(@Hashes)";

            return db.Query<MatchResult>(sql, new { Hashes = hashes.ToList() });
        }
    }
}