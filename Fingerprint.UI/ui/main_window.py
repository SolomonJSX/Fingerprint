import sys
import requests
from PyQt6.QtWidgets import (QMainWindow, QWidget, QVBoxLayout, QHBoxLayout, 
                             QLineEdit, QPushButton, QLabel, QFrame, QListWidgetItem, QListWidget)
from PyQt6.QtCore import Qt, QSize

import webbrowser

from utils.youtube_search_worker import YoutubeSearchWorker

from utils.AddSongWorker import AddSongWorker
from utils.RecognizeWorker import RecognizeWorker
from ui.video_item import VideoItemWidget
from scrapers.youtube_helper import YoutubeHelper

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Fingerprint Shazam")
        self.setFixedSize(400, 650)
        self.init_ui()

    def init_ui(self):
        central_widget = QWidget()
        central_widget.setObjectName("centralWidget")
        self.setCentralWidget(central_widget)
        
        # Основной макет теперь без жестких ограничений
        self.main_layout = QVBoxLayout(central_widget)
        self.main_layout.setContentsMargins(25, 30, 25, 30)
        self.main_layout.setSpacing(15)

        # 1. Секция распознавания
        self.btn_recognize = QPushButton("F")
        self.btn_recognize.setObjectName("recognizeButton")
        self.btn_recognize.setFixedSize(160, 160)
        self.btn_recognize.clicked.connect(self.start_recognition)
        self.main_layout.addWidget(self.btn_recognize, alignment=Qt.AlignmentFlag.AlignCenter)

        self.label_hint = QLabel("Нажми, чтобы распознать")
        self.label_hint.setObjectName("hintLabel")
        self.label_hint.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.main_layout.addWidget(self.label_hint)

        # Статус распознавания (перенесли выше списка, чтобы не было наслоения)
        self.label_status = QLabel("")
        self.label_status.setObjectName("statusLabel")
        self.label_status.setWordWrap(True)
        self.label_status.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.label_status.setMinimumHeight(40)
        self.main_layout.addWidget(self.label_status)

        line = QFrame()
        line.setFrameShape(QFrame.Shape.HLine)
        line.setObjectName("separator")
        self.main_layout.addWidget(line)

        # 2. Секция YouTube (появляется динамически)
        self.label_results = QLabel("Похожие видео на YouTube:")
        self.label_results.setObjectName("sectionLabel")
        self.label_results.hide() 
        self.main_layout.addWidget(self.label_results)

        self.list_videos = QListWidget()
        self.list_videos.setObjectName("videoList")
        self.list_videos.hide()
        self.list_videos.setFixedHeight(220) # Фиксируем высоту списка
        
        # ВКЛЮЧАЕМ СКРОЛЛ ЯВНО
        self.list_videos.setVerticalScrollBarPolicy(Qt.ScrollBarPolicy.ScrollBarAsNeeded)
        self.list_videos.setHorizontalScrollBarPolicy(Qt.ScrollBarPolicy.ScrollBarAlwaysOff)
        self.list_videos.setVerticalScrollMode(QListWidget.ScrollMode.ScrollPerPixel) # Плавный скролл
        
        self.list_videos.itemClicked.connect(lambda item: webbrowser.open(item.data(Qt.ItemDataRole.UserRole)))
        self.main_layout.addWidget(self.list_videos)

        # 3. Секция добавления по URL
        self.label_add = QLabel("Добавить в базу по URL:")
        self.label_add.setObjectName("sectionLabel")
        self.main_layout.addWidget(self.label_add)

        self.input_url = QLineEdit()
        self.input_url.setPlaceholderText("Вставь ссылку здесь...")
        self.input_url.setObjectName("urlInput")
        self.main_layout.addWidget(self.input_url)

        self.btn_add = QPushButton("Добавить в библиотеку")
        self.btn_add.setObjectName("addButton")
        self.btn_add.clicked.connect(self.start_adding_process)
        self.main_layout.addWidget(self.btn_add)

        # Убираем жесткий fixedSize, ставим минимальный
        self.setMinimumWidth(400)
        self.setMinimumHeight(650)
        self.adjustSize() # Подгоняем размер под контент

        # ПРИМЕНЕНИЕ СТИЛЕЙ (QSS)
        self.setStyleSheet("""
            #centralWidget {
                background-color: #121212;
            }
            #recognizeButton {
                background-color: qlineargradient(x1:0, y1:0, x2:1, y2:1, stop:0 #00d2ff, stop:1 #3a7bd5);
                color: white;
                border-radius: 90px;
                font-size: 80px;
                font-weight: bold;
                border: 4px solid #1db954;
            }
            #recognizeButton:hover {
                border: 4px solid white;
            }
            #hintLabel {
                color: #b3b3b3;
                font-size: 14px;
            }
            #sectionLabel {
                color: white;
                font-weight: bold;
                font-size: 16px;
            }
            #urlInput {
                background-color: #282828;
                border: 1px solid #3e3e3e;
                border-radius: 8px;
                padding: 10px;
                color: white;
                font-size: 13px;
            }
            #addButton {
                background-color: #1db954;
                color: white;
                border-radius: 8px;
                padding: 12px;
                font-weight: bold;
                font-size: 14px;
            }
            #addButton:hover {
                background-color: #1ed760;
            }
            #statusLabel {
                color: #1db954;
                font-style: italic;
            }
            #separator {
                background-color: #3e3e3e;
            }
                           QScrollBar:vertical {
                border: none;
                background: #181818;
                width: 8px;
                margin: 0px;
            }
            #centralWidget {
                background-color: #121212;
            }
            
            /* Стили для самого списка */
            #videoList {
                background-color: #181818;
                border: 1px solid #333333;
                border-radius: 10px;
                outline: none; /* Убирает пунктирную рамку */
            }

            /* Стиль для каждого элемента списка */
            #videoList::item {
                background-color: #181818;
                border-bottom: 1px solid #252525;
            }

            /* Когда наводим мышкой на видео */
            #videoList::item:hover {
                background-color: #282828;
            }

            /* Когда видео выбрано (кликнули) */
            #videoList::item:selected {
                background-color: #333333;
                border-left: 3px solid #1db954; /* Зеленая полоска слева как в Spotify */
            }

            /* Настройка скроллбара (чтобы он был виден на черном) */
            QScrollBar:vertical {
                border: none;
                background: #121212;
                width: 10px;
                margin: 0px;
            }
            QScrollBar::handle:vertical {
                background: #333;
                min-height: 20px;
                border-radius: 5px;
            }
            QScrollBar::handle:vertical:hover {
                background: #1db954;
            }
        """)

    def start_adding_process(self):
        url = self.input_url.text().strip()
        if not url:
            self.label_status.setText("Введите ссылку!")
            return

        self.label_status.setText("Обработка... Подождите.")
        self.btn_add.setEnabled(False)

        # Запуск рабочего потока
        self.worker = AddSongWorker(url)
        self.worker.finished.connect(self.on_process_finished)
        self.worker.start()

    def on_process_finished(self, message):
        self.btn_add.setEnabled(True)
        self.label_status.setText(message)
        self.input_url.clear()

    # Новые методы в классе MainWindow
    def start_recognition(self):
        self.label_hint.setText("Слушаю внимательно (10 сек)...")
        self.btn_recognize.setEnabled(False)
        # Можно добавить визуальный эффект, например смену цвета
        self.btn_recognize.setStyleSheet("background-color: #ff4b2b; border-radius: 90px; color: white; font-size: 80px;")
        
        self.rec_worker = RecognizeWorker()
        self.rec_worker.finished.connect(self.on_recognition_finished)
        self.rec_worker.start()

    def on_recognition_finished(self, result):
        self.btn_recognize.setEnabled(True)
        self.btn_recognize.setStyleSheet("""
            background-color: qlineargradient(x1:0, y1:0, x2:1, y2:1, stop:0 #00d2ff, stop:1 #3a7bd5);
            color: white; 
            border-radius: 90px; 
            font-size: 80px; 
            font-weight: bold; 
            border: 4px solid #1db954;
        """) # Твой стандартный стиль

        if "error" in result:
            self.label_hint.setText(f"❌ {result['error']}")
            return

        artist = result.get('artist') or result.get('Artist', 'Неизвестно')
        title = result.get('title') or result.get('Title', 'Неизвестно')
        
        self.label_status.setText(f"🏆 {artist} — {title}")
        self.label_hint.setText("✅ Песня найдена! Ищу видео...") # Даем понять пользователю, что процесс идет

        # Запускаем ПОТОК поиска YouTube вместо прямого вызова
        query = f"{artist} {title} official music video"
        self.yt_worker = YoutubeSearchWorker(query)
        self.yt_worker.finished.connect(self.display_youtube_results) # Новый метод для вывода
        self.yt_worker.start()

    def display_youtube_results(self, videos):
        """Этот метод сработает, когда поиск в YouTube завершится"""
        self.list_videos.clear()

        if videos:
            self.label_results.show()
            self.list_videos.show()
            for vid in videos:
                item = QListWidgetItem(self.list_videos)
                item.setSizeHint(QSize(0, 80))
                item.setData(Qt.ItemDataRole.UserRole, vid['link'])
                
                video_widget = VideoItemWidget(vid['title'], vid['duration'], vid['thumbnail'])
                self.list_videos.setItemWidget(item, video_widget)
        else:
            self.label_results.hide()
            self.list_videos.hide()
            self.label_hint.setText("✅ Песня найдена, но видео не найдены.")
        
        self.adjustSize()

if __name__ == "__main__":
    from PyQt6.QtWidgets import QApplication
    app = QApplication(sys.argv)
    window = MainWindow()
    window.show()
    sys.exit(app.exec())