using Fingerprint.Core.Algorithms;
using Fingerprint.Infrastructure.Algorithms;
using Fingerprint.Infrastructure.Services;

string connString = "Host=localhost;Username=postgres;Password=super2015;Database=Fingerprint_db";
var storage = new StorageService(connString);

string mp3File = @"D:\VS Projects\Fingerprint\FingerPrint.Main\musicTest\NEFFEX_Cold.mp3";
string artist = "NEFFEX";
string title = "Cold";

Console.WriteLine($"Обработка файла {mp3File}...");

try 
{
    // 1. Сохраняем информацию о песне и получаем ID
    // Передаем 0 как временный ID, так как реальный ID мы получим из базы
    int songId = storage.SaveSong(artist, title); 
    Console.WriteLine($"Песня сохранена с ID: {songId}");

    // 2. Генерируем отпечатки
    // Важно: в FingerprintAudio мы передаем (uint)songId
    var fingerprints = Fingerprinter.FingerprintAudio(mp3File, songId);
    Console.WriteLine($"Сгенерировано {fingerprints.Count} хешей.");

    // 3. Сохраняем отпечатки в БД
    storage.SaveFingerprints(fingerprints, songId);
    Console.WriteLine("Отпечатки успешно загружены в базу!");
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}