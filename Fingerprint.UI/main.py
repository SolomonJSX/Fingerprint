from scrapers.hitmo_scraper import HitmoScraper

def main():
    # URL, который мы хотим проверить
    test_url = "https://eu.hitmo-top.com/song/48004576"
    
    # Вызываем метод нашего класса
    result = HitmoScraper.get_track_info(test_url)
    
    if result:
        print(f"В main.py получены данные: {result.artist} - {result.title}. downloadUrl: {result.download_url}")
    else:
        print("Не удалось получить данные в main.py")

if __name__ == "__main__":
    main()