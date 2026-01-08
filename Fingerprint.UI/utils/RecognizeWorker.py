import sounddevice as sd
from scipy.io.wavfile import write
import tempfile
from PyQt6.QtCore import Qt, QThread, pyqtSignal
import os
import requests

class RecognizeWorker(QThread):
    finished = pyqtSignal(dict) # Передаем словарь с результатом от сервера

    def run(self):
        try:
            fs = 44100  # Частота дискретизации (как в твоем C# коде)
            seconds = 10  # Длительность записи
            
            # 1. Запись с микрофона
            recording = sd.rec(int(seconds * fs), samplerate=fs, channels=1, dtype='int16')
            sd.wait()  # Ждем завершения записи
            
            # 2. Сохраняем во временный WAV файл
            with tempfile.NamedTemporaryFile(delete=False, suffix=".wav") as tmp:
                temp_filename = tmp.name
                write(temp_filename, fs, recording)

            # 3. Отправляем файл на бэкенд
            api_url = "http://localhost:5182/api/Recognition/identify"
            with open(temp_filename, 'rb') as f:
                files = {'audioFile': ('query.wav', f, 'audio/wav')}
                response = requests.post(api_url, files=files)
            
            # Удаляем временный файл
            os.remove(temp_filename)

            if response.status_code == 200:
                self.finished.emit(response.json())
            elif response.status_code == 404:
                self.finished.emit({"error": "Песня не найдена"})
            else:
                self.finished.emit({"error": f"Ошибка сервера: {response.status_code}"})

        except Exception as e:
            self.finished.emit({"error": str(e)})