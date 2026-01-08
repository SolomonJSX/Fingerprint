from models.TrackData import TrackData
import requests

class ApiClient:
    def __init__(self, base_url = "https://localhost:7060//api"):
        self.base_url = base_url

    def add_song_by_url(self, track: TrackData):
        url = f"{self.base_url}/Recognition/add-song"
        
        # Данные в точном соответствии с AddSongRequest в C#
        payload = {
            "DownloadUrl": track.download_url,
            "Artist": track.artist,
            "Title": track.title
        }

        try:
            print(f"Отправка запрос на сервер для: {track.title}...")
            response = requests.post(url, json=payload, timeout=60)

            response.raise_for_status()

            result = response.json()

            print(f"Сервер ответил: {result['message']} (ID: {result['songId']})")
            return True
        except Exception as ex:
            print(f"Ошибка при связи с API: {ex}")
            return False
