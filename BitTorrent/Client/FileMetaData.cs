namespace Server;

public class FileMetaData
{
    public string RootHash { get; set; }
    public string FileName { get; set; }
    public string FileExtension { get; set; }
    public int FileSize { get; set; }
    public int BlockSize { get; set; }  
}