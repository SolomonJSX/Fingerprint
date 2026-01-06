import requests
from bs4 import BeautifulSoup
from models.TrackData import TrackData

class HitmoScraper:
    @staticmethod
    def get_track_info(url: str) -> TrackData:
        headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36'
        }
        try:
            response = requests.get(url, headers=headers, timeout=10)
            response.raise_for_status()
            soup = BeautifulSoup(response.text, "html.parser")

            track_info = soup.find("div", class_="track__info")
            download_btn = soup.find("a", class_="track__download-btn")
            
            if not track_info or not download_btn:
                return TrackData(error_message="Элементы страницы не найдены", success=False)

            title = track_info.select_one(".track__title").get_text(strip=True)
            artist = track_info.select_one(".track__desc").get_text(strip=True)
            download_url = download_btn.get("href")

            return TrackData(
                artist=artist, 
                title=title, 
                download_url=download_url, 
                success=True
            )
            
        except Exception as e:
            return TrackData(error_message=str(e), success=False)