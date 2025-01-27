using YoutubeDownloader;

Youtube youtube = new Youtube();

Console.Write("Need to delete audio file(Y/N)?: ");
youtube.isDeleteAudioFile = Console.ReadLine()?.ToLower() == "y";

string exit = "y";
do
{
    youtube.SetYoutubeUrl();
    youtube.GetVideo();
    Console.WriteLine("\n--------------------------------------------------------------------------------------");
    Console.Write("Exit? (Y/N): ");
    exit = Console.ReadLine() ?? "n";
} while (exit?.ToLower() == "n");
