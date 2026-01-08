from PyQt6.QtCore import Qt, QThread, pyqtSignal, QSize
from scrapers.hitmo_scraper import HitmoScraper as TrackInfo
import requests

class AddSongWorker(QThread):
    finished = pyqtSignal(str)

    def __init__(self, url):
        super().__init__()
        self.url = url

    def run(self):
        try:
            # 1. Парсим данные
            track = TrackInfo.get_track_info(self.url)
            if not track:
                self.finished.emit("Ошибка: Не удалось найти песню по ссылке.")
                return
            
            # 2. Отправляем на C# Бэкенд
            api_url = "http://localhost:5182/api/Recognition/add-song"
            payload = {
                "downloadUrl": track.download_url,
                "artist": track.artist,
                "title": track.title
            }
            
            response = requests.post(api_url, json=payload, timeout=60)
            if response.status_code == 200:
                self.finished.emit(f"Успешно добавлено: {track.artist} - {track.title}")
            else:
                self.finished.emit(f"Ошибка сервера: {response.status_code}")
                
        except Exception as e:
            self.finished.emit(f"Произошла ошибка: {str(e)}")
