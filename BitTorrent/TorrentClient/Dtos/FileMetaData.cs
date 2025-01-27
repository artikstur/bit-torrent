using MerkleTree;

namespace TorrentClient;
using static PackageHelper;

public class FileMetaData
{
    public string RootHash { get; set; }
    public FileStatus FileStatus { get; set; }
    public string FileName { get; set; }
    public string FileExtension { get; set; }
    public long FileSize { get; set; }
    public long BlockSize { get; set; }
    public long TotalBlocks { get; set; } 
    // Если FileStatus = Sharing, то AlreadyDownloaded = FileSize
    public int AlreadyDownloaded { get; set; }
    // Путь на текущем компьютере (Не должен передаваться другим клиентам)
    public string FilePath { get; set; }
    public List<byte[]> Blocks { get; set; }
}