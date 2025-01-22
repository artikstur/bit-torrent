namespace TorrentClient;

public class FileMetaData
{
    public string RootHash { get; set; }
    public FileStatus FileStatus { get; set; }
    public string FileName { get; set; }
    public string FileExtension { get; set; }
    public int FileSize { get; set; }
    public int BlockSize { get; set; }
    // Если FileStatus = Sharing, то AlreadyDownloaded = FileSize
    public int AlreadyDownloaded { get; set; }
    // Путь на текущем компьютере (Не должен передаваться другим клиентам)
    public string FilePath { get; set; }
}