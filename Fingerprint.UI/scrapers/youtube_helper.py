from youtubesearchpython import VideosSearch

class YoutubeHelper:
    @staticmethod
    def search_videos(query: str, limit=5):
        """Ищет видео на YouTube по запросу"""
        print(f"Поиск в YouTube для: {query}")

        try:
            search = VideosSearch(query, limit)

            results = search.result()['result']

            video_list = []
            for video in results:
                video_list.append({
                    'title': video['title'],
                    'link': video['link'],
                    'thumbnail': video['thumbnails'][0]['url'], # Ссылка на превью
                    'duration': video['duration']
                })
            return video_list
        except Exception as e:
            print(f"Ошибка поиска YouTube: {e}")
            return []
