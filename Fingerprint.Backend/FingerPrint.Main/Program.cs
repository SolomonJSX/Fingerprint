using Fingerprint.Core.Services;
using Fingerprint.Infrastructure.Algorithms;
using Fingerprint.Infrastructure.Services;
using Fingerprint.Infrastructure.Services.Implementations;

string connString = "Host=localhost;Username=postgres;Password=super2015;Database=Fingerprint_db";
var storage = new StorageService(connString);

string mp3File = @"D:\VS Projects\Fingerprint\FingerPrint.Main\musicTest\Ninety_One_ajjtadan_78110868.mp3";
string artist = "Ninety One";
string title = "Кайтадан";

/*Console.WriteLine($"Обработка файла {mp3File}...");

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
}*/

// --- СЦЕНАРИЙ: РАСПОЗНАВАНИЕ ---
try 
{
    Console.WriteLine("\n--- ТЕСТ РАСПОЗНАВАНИЯ ---");
    var recognitionService = new RecognitionService(storage);

    // Допустим, мы "услышали" этот файл
    string testFile = @"D:\VS Projects\Fingerprint\Fingerprint.Backend\FingerPrint.Main\musicTest\Call_Me_Maybe_48004576.mp3";
    
    // Генерируем хеши для него (ID 0, т.к. мы еще не знаем кто это)
    var queryHashes = Fingerprinter.FingerprintAudio(testFile, 0);
    
    var result = recognitionService.Match(queryHashes);

    if (result != null)
    {
        Console.WriteLine($"УСПЕХ! Найдена песня: {result.Artist} - {result.Title}");
        Console.WriteLine($"Коэффициент уверенности (Score): {result.Score}");
    }
    else
    {
        Console.WriteLine("Песня не найдена в базе данных.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка при распознавании: {ex.Message}");
}