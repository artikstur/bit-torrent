using MerkleTree;

namespace TorrentClient;

public class FileMetaData
{
    public string RootHash { get; set; }
    public FileStatus FileStatus { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public long BlockSize { get; set; }
    public long TotalBlocks { get; set; } 
    public string FilePath { get; set; }
    public byte[][] Blocks { get; set; } 
}