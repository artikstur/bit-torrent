namespace TorrentClient;

public static class FileWorker
{
    public static List<byte[]> SplitFileIntoBlocks(FileMetaData fileMetaData, int blockSize)
    {
        List<byte[]> blocks = new List<byte[]>();

        using FileStream fileStream = new FileStream(fileMetaData.FilePath, FileMode.Open, FileAccess.Read);
        byte[] buffer = new byte[blockSize];
        int bytesRead;
            
        while ((bytesRead = fileStream.Read(buffer, 0, blockSize)) > 0)
        {
            if (bytesRead < blockSize)
            {
                byte[] actualBlock = new byte[bytesRead];
                Array.Copy(buffer, actualBlock, bytesRead);
                blocks.Add(actualBlock);
            }
            else
            {
                blocks.Add(buffer);
            }
        }

        return blocks;
    }
    
    public static long GetFileSize(FileMetaData fileMetaData)
    {
        if (File.Exists(fileMetaData.FilePath))
        {
            return new FileInfo(fileMetaData.FilePath).Length;
        }
        
        throw new FileNotFoundException("Файл не найден", fileMetaData.FilePath);
    }
}