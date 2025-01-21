namespace Client;

public class DownloadingFile
{
    public string RootHash { get; set; }
    public string FileName { get; set; }
    public string FileExtension { get; set; }
    public int FileSize { get; set; }
    public int BlockSize { get; set; }
    public int AlreadyDownloaded { get; set; }
    public string FilePath { get; set; }
}