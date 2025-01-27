using System.Diagnostics;
using System.Text.Json;

namespace YoutubeDownloader
{
	public class Youtube
	{
		public string VideoUrl { get; set; } = null!;
		public string AudioFilePath { get; set; } = null!;
		public string VideoFilePath { get; set; } = null!;
		public string FinalVideoFilePath { get; set; } = null!;
		public string ytDlpPath { 
			get 
			{ 
				return "yt-dlp.exe"; 
			} 
		}

		public void SetYoutubeUrl()
		{
			Console.Write("Enter the YouTube video URL:");
			this.VideoUrl = Console.ReadLine() ?? string.Empty;
			//this.VideoUrl = $"--no-check-certificate {this.VideoUrl}";
		}

		public void GetVideo()
		{
			FinalVideoFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

			try
			{
				// Set up the process to call yt-dlp
				var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = ytDlpPath,
						Arguments = $"-f bestvideo+bestaudio -o \"{FinalVideoFilePath}\" {VideoUrl}",
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						UseShellExecute = false,
						CreateNoWindow = true
					}
				};

				// Start the process and capture output
				process.Start();
				string output = process.StandardOutput.ReadToEnd();
				string error = process.StandardError.ReadToEnd();
				process.WaitForExit();

				if (process.ExitCode == 0)
				{
					Console.WriteLine($"Download completed successfully! Saved as {FinalVideoFilePath}");
				}
				else
				{
					Console.WriteLine("An error occurred:");
					Console.WriteLine(error);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An unexpected error occurred: {ex.Message}");
			}
		}

		public void GetVideoDetails()
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
					return;
				}

				// Step 2: Parse JSON metadata
				var videoDetails = JsonSerializer.Deserialize<VideoDetails>(metadataJson);

				if (videoDetails == null)
				{
					Console.WriteLine("Failed to parse video metadata. Exiting...");
					return;
				}
				else
				{
					// Step 3: Display video details
					Console.WriteLine($"Title: {videoDetails.Title}");
					Console.WriteLine($"Uploader: {videoDetails.Uploader}");
					Console.WriteLine($"Duration: {FormatDuration(videoDetails.Duration)}");
					Console.WriteLine("Available Formats:");

					Console.WriteLine("Format ID\t\tType\t\tResolution\t\tCodec\t\tSize");

					videoDetails?.Formats?.ForEach(format =>
					{
						//Console.WriteLine($"- Format ID: {format.FormatId}");
						//Console.WriteLine($"  Type: {(format.Vcodec == "none" ? "Audio" : "Video")}");
						//Console.WriteLine($"  Resolution: {format.Resolution}");
						//Console.WriteLine($"  Codec: {format.Acodec}/{format.Vcodec}");
						//Console.WriteLine($"  Size: {FormatFileSize(format.Filesize)}");
						Console.WriteLine($"{format.FormatId}\t {(format.Vcodec == "none" ? "Audio" : "Video")}\t {format.Resolution} \t {format.Acodec}/{format.Vcodec}\t{FormatFileSize(format.Filesize)} ");
					});

					Console.WriteLine("Use the format ID to select a specific format for download.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An unexpected error occurred: {ex.Message}");
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

		// Helper to format duration
		private static string FormatDuration(int? seconds)
		{
			if (seconds == null) return "Unknown";
			TimeSpan time = TimeSpan.FromSeconds(seconds.Value);
			return time.ToString(@"hh\:mm\:ss");
		}
	}
}
