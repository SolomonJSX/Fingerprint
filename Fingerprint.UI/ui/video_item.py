from PyQt6.QtWidgets import QWidget, QHBoxLayout, QVBoxLayout, QLabel
from PyQt6.QtGui import QPixmap, QImage
from PyQt6.QtCore import Qt
import requests

class VideoItemWidget(QWidget):
    def __init__(self, title, duration, thumbnail_url, parent=None):
        super().__init__(parent)

        self.setAttribute(Qt.WidgetAttribute.WA_TranslucentBackground)
        
        # Главный горизонтальный слой
        layout = QHBoxLayout(self)
        layout.setContentsMargins(5, 5, 5, 5)
        layout.setSpacing(15)

        # 1. ПРЕВЬЮ (Картинка)
        self.thumbnail_label = QLabel()
        self.thumbnail_label.setFixedSize(120, 68) # Формат 16:9
        self.thumbnail_label.setStyleSheet("border-radius: 4px; background-color: #000;")
        self.load_image(thumbnail_url)
        layout.addWidget(self.thumbnail_label)

        # 2. ТЕКСТОВАЯ ИНФОРМАЦИЯ (Вертикальный слой)
        text_layout = QVBoxLayout()
        
        self.title_label = QLabel(title)
        self.title_label.setWordWrap(True)
        self.title_label.setStyleSheet("""
            color: #FFFFFF; 
            font-weight: bold; 
            font-size: 13px;
            background: transparent;
        """)
        
        self.duration_label = QLabel(f"Длительность: {duration}")
        self.duration_label.setStyleSheet("""
            color: #AAAAAA; 
            font-size: 11px;
            background: transparent;
        """)
        
        text_layout.addWidget(self.title_label)
        text_layout.addWidget(self.duration_label)
        text_layout.addStretch()
        
        layout.addLayout(text_layout)

    def load_image(self, url):
        """Загружает картинку по URL и ставит в QLabel"""
        try:
            response = requests.get(url, timeout=5)
            image = QImage()
            image.loadFromData(response.content)
            pixmap = QPixmap.fromImage(image)
            # Масштабируем под размер лейбла
            self.thumbnail_label.setPixmap(pixmap.scaled(
                self.thumbnail_label.size(), 
                Qt.AspectRatioMode.KeepAspectRatioByExpanding, 
                Qt.TransformationMode.SmoothTransformation
            ))
        except Exception as e:
            print(f"Ошибка загрузки превью: {e}")