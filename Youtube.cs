using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace YoutubeDownloader
{
	public class Youtube
	{
		public string VideoUrl { get; set; } = null!;
		public string? _fileName { get; set; }
		public string FileName 
		{ 
			get
			{
				return RemoveInvalidCharacters(_fileName ?? string.Empty);
            }
		}
		public string AudioFilePath { get; set; } = null!;
		public string VideoFilePath { get; set; } = null!;
		public string FinalVideoFilePath 
		{ 
			get 
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            } 
		}
		public VideoDetails? VideoDetails { get; set; }
		public string ytDlpPath { 
			get 
			{ 
				return "yt-dlp.exe"; 
			} 
		}
        public string ffmpegPath
		{
			get
			{
				return Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "bin", "ffmpeg.exe");
			}
		}
        public void SetYoutubeUrl()
		{
			Console.Write("Enter the YouTube video URL:");
			this.VideoUrl = Console.ReadLine() ?? string.Empty;
			//this.VideoUrl = $"--no-check-certificate {this.VideoUrl}";
		}
		public bool isDeleteAudioFile { get; set; } = false;

        public void GetVideo()
		{
			GetVideoDetails();

            Console.Write("Enter the format code for the desired quality(video): ");
            var formatCodeVideo = Console.ReadLine();

            Console.Write("Enter the format code for the desired quality(audio): ");
            var formatCodeAudio = Console.ReadLine();

			var videoDetail = VideoDetails?.Formats?.FirstOrDefault(x => formatCodeVideo == x.FormatId?.Trim().ToLower());
			var audioDetail = VideoDetails?.Formats?.FirstOrDefault(x => formatCodeAudio == x.FormatId?.Trim().ToLower());

            var outputTempDirectory = Path.Combine(FinalVideoFilePath, "Temp");
            var finalOutputPath = Path.Combine(FinalVideoFilePath, $"{FileName} ({videoDetail?.Resolution}).mp4");
            Directory.CreateDirectory(outputTempDirectory);

            VideoFilePath = Path.Combine(FinalVideoFilePath, outputTempDirectory, $"{Guid.NewGuid().ToString("N")}_video.mp4");
			AudioFilePath = Path.Combine(FinalVideoFilePath, outputTempDirectory, $"{Guid.NewGuid().ToString("N")}_audio.mp3");

            DownloadAsync(formatCodeVideo, VideoFilePath, VideoUrl);
            DownloadAsync(formatCodeAudio, AudioFilePath, VideoUrl);

            if (File.Exists(VideoFilePath) && File.Exists(AudioFilePath))
            {
                Console.WriteLine("\nCombining video and audio...");
                CombineVideoAndAudio(VideoFilePath, AudioFilePath, finalOutputPath);
                Console.WriteLine($"Final output: {finalOutputPath}");
            }
            else
            {
                Console.WriteLine("Failed to download video or audio.");
            }

            File.Delete(VideoFilePath);
			if (isDeleteAudioFile)
			{
				File.Delete(AudioFilePath);
			}
        }

		public VideoDetails? GetVideoDetails()
		{
			try
			{
				// Step 1: Fetch video metadata as JSON
				Console.WriteLine("Fetching video details...");
				var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = ytDlpPath,
						Arguments = $"-j {VideoUrl}",
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						UseShellExecute = false,
						CreateNoWindow = true
					}
				};

				process.Start();
				string metadataJson = process.StandardOutput.ReadToEnd();
				string errorOutput = process.StandardError.ReadToEnd();
				process.WaitForExit();

				if (process.ExitCode != 0)
				{
					Console.WriteLine("Error fetching video details:");
					Console.WriteLine(errorOutput);
					return VideoDetails;
				}

				// Step 2: Parse JSON metadata
				VideoDetails = JsonSerializer.Deserialize<VideoDetails>(metadataJson);

				if (VideoDetails == null)
				{
					Console.WriteLine("Failed to parse video metadata. Exiting...");
					return VideoDetails;
				}
				else
				{
					this._fileName = VideoDetails.Title?.Replace("| ", "").Replace("|","");
					
					// Step 3: Display video details
					Console.WriteLine($"Title: {VideoDetails.Title}");
					Console.WriteLine($"Uploader: {VideoDetails.Uploader}");
					Console.WriteLine($"Duration: {FormatDuration(VideoDetails.Duration)}");
					Console.WriteLine("Available Formats:");

					Console.WriteLine("Format ID\tType\t\tResolution\t\t\tCodec");

                    VideoDetails?.Formats?.ForEach(format =>
					{
						Console.WriteLine($"{format.FormatId}\t\t {(format.Vcodec == "none" ? "Audio" : "Video")}\t\t {format.Resolution} - {FormatFileSize(format.Filesize)} \t\t {format.Acodec}/{format.Vcodec} ");
					});

					Console.WriteLine("Use the format ID to select a specific format for download.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An unexpected error occurred: {ex.Message}");
			}

			return VideoDetails;
        }

        private void DownloadAsync(string formatCode, string filepath, string videoUrl)
        {
            Console.WriteLine($"\nDownloading video in format {formatCode}...");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ytDlpPath,
                    Arguments = $"-f {formatCode} -o \"{filepath}\" {videoUrl}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string downloadOutput = process.StandardOutput.ReadToEnd();
            string downloadError = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine($"Download completed successfully! Saved as {filepath}");
            }
            else
            {
                Console.WriteLine("An error occurred during download:");
                Console.WriteLine(downloadError);
            }
        }

        private void CombineVideoAndAudio(string videoPath, string audioPath, string outputPath)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = $"-i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a aac \"{outputPath}\" -y",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"FFmpeg error: {output}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while combining video and audio: {ex.Message}");
            }
        }

        private static string FormatFileSize(long? size)
		{
			if (size == null) return "Unknown";
			double fileSize = size.Value;
			string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
			int suffixIndex = 0;

			while (fileSize >= 1024 && suffixIndex < suffixes.Length - 1)
			{
				fileSize /= 1024;
				suffixIndex++;
			}

			return $"{fileSize:F2} {suffixes[suffixIndex]}";
		}

		private static string FormatDuration(int? seconds)
		{
			if (seconds == null) return "Unknown";
			TimeSpan time = TimeSpan.FromSeconds(seconds.Value);
			return time.ToString(@"hh\:mm\:ss");
		}

        private static string RemoveInvalidCharacters(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

            fileName = fileName.Replace("| ", string.Empty).Replace("|", string.Empty);

            // Define the characters to remove
            char[] invalidChars = { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };

            var sanitizedFileName = new StringBuilder();
            foreach (char c in fileName)
            {
                // Append only characters not in the invalid character list
                if (Array.IndexOf(invalidChars, c) == -1)
                {
                    sanitizedFileName.Append(c);
                }
            }

            return sanitizedFileName.ToString();
        }
    }
}
