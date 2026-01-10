from PyQt6.QtCore import Qt, QThread, pyqtSignal
from scrapers.youtube_helper import YoutubeHelper

class YoutubeSearchWorker(QThread):
    finished = pyqtSignal(list)

    def __init__(self, query):
        super().__init__()
        self.query = query # Сохраняем как self.query

    def run(self):
        try:
            # Используем то же имя: self.query
            print(f"Поток YouTube запущен для запроса: {self.query}")
            videos = YoutubeHelper.search_videos(self.query)
            self.finished.emit(videos)
        except Exception as e:
            print(f"Ошибка в потоке YouTube: {e}")
            self.finished.emit([])