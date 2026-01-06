from dataclasses import dataclass
from typing import Optional

@dataclass
class TrackData:
    artist: str
    title: str
    download_url: str
    success: bool = True
    error_message: Optional[str] = None

    def __str__(self):
        return f"{self.artist} - {self.title}"