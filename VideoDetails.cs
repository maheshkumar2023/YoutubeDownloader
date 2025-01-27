using System.Text.Json.Serialization;

namespace YoutubeDownloader
{
	// Classes to parse yt-dlp JSON output
	public class VideoDetails
	{
		[JsonPropertyName("title")]
		public string? Title { get; set; }

		[JsonPropertyName("uploader")]
		public string? Uploader { get; set; }

		[JsonPropertyName("duration")]
		public int? Duration { get; set; }

		[JsonPropertyName("formats")]
		public List<FormatDetails>? Formats { get; set; }
	}

	public class FormatDetails
	{
		[JsonPropertyName("format_id")]
		public string? FormatId { get; set; }

		[JsonPropertyName("format_note")]
		public string? FormatNote { get; set; }

		[JsonPropertyName("filesize")]
		public long? Filesize { get; set; }

		[JsonPropertyName("acodec")]
		public string? Acodec { get; set; }

		[JsonPropertyName("vcodec")]
		public string? Vcodec { get; set; }

		[JsonPropertyName("resolution")]
		public string? Resolution { get; set; }
	}
}
