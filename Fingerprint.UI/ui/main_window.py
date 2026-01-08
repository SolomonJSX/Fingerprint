import sys
import requests
from PyQt6.QtWidgets import (QMainWindow, QWidget, QVBoxLayout, QHBoxLayout, 
                             QLineEdit, QPushButton, QLabel, QFrame)
from PyQt6.QtCore import Qt

from utils.AddSongWorker import AddSongWorker
from utils.RecognizeWorker import RecognizeWorker

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Fingerprint Shazam")
        self.setFixedSize(400, 650)
        self.init_ui()

    def init_ui(self):
        # Центральный виджет и главный макет
        central_widget = QWidget()
        central_widget.setObjectName("centralWidget")
        self.setCentralWidget(central_widget)
        layout = QVBoxLayout(central_widget)
        layout.setContentsMargins(30, 40, 30, 40)
        layout.setSpacing(20)

        # 1. ОГРОМНАЯ КРУГЛАЯ КНОПКА (Recognition)
        self.btn_recognize = QPushButton("F")
        self.btn_recognize.setObjectName("recognizeButton")
        self.btn_recognize.setFixedSize(180, 180)
        self.btn_recognize.clicked.connect(self.start_recognition)
        layout.addWidget(self.btn_recognize, alignment=Qt.AlignmentFlag.AlignCenter)

        self.label_hint = QLabel("Нажми, чтобы распознать")
        self.label_hint.setObjectName("hintLabel")
        self.label_hint.setAlignment(Qt.AlignmentFlag.AlignCenter)
        layout.addWidget(self.label_hint)

        # Разделитель
        line = QFrame()
        line.setFrameShape(QFrame.Shape.HLine)
        line.setObjectName("separator")
        layout.addWidget(line)

        # 2. СЕКЦИЯ ДОБАВЛЕНИЯ ПО URL
        self.label_add = QLabel("Добавить в базу по URL:")
        self.label_add.setObjectName("sectionLabel")
        layout.addWidget(self.label_add)

        self.input_url = QLineEdit()
        self.input_url.setPlaceholderText("https://eu.hitmo-top.com/song/...")
        self.input_url.setObjectName("urlInput")
        layout.addWidget(self.input_url)

        self.btn_add = QPushButton("Добавить песню")
        self.btn_add.setObjectName("addButton")
        self.btn_add.clicked.connect(self.start_adding_process)
        layout.addWidget(self.btn_add)

        # 3. СТАТУС БАР
        self.label_status = QLabel("")
        self.label_status.setObjectName("statusLabel")
        self.label_status.setWordWrap(True)
        self.label_status.setAlignment(Qt.AlignmentFlag.AlignCenter)
        layout.addStretch() # Прижимает статус к низу
        layout.addWidget(self.label_status)

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
        self.btn_recognize.setStyleSheet("...") # твой стиль кнопки
        
        print(f"DEBUG: Получен ответ от сервера: {result}") # Это поможет отладить

        if "error" in result:
            # Если пришла ошибка (например 404 или 500)
            self.label_hint.setText(f"❌ {result['error']}")
            self.label_status.setText("") # Очищаем старый результат!
        else:
            # Если песня найдена
            self.label_hint.setText("✅ Песня найдена!")
            # Проверь регистр ключей! В C# они могут быть Artist или artist
            artist = result.get('artist') or result.get('Artist', 'Неизвестно')
            title = result.get('title') or result.get('Title', 'Неизвестно')
            score = result.get('score') or result.get('Score', 0)
            
            self.label_status.setText(f"🏆 {artist} — {title}\n(Точность: {score})")

if __name__ == "__main__":
    from PyQt6.QtWidgets import QApplication
    app = QApplication(sys.argv)
    window = MainWindow()
    window.show()
    sys.exit(app.exec())