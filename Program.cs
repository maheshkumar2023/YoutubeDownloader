using YoutubeDownloader;

Console.WriteLine("Youtube Video Downloader!");

Youtube youtube = new Youtube();
youtube.SetYoutubeUrl();
youtube.GetVideoDetails();
