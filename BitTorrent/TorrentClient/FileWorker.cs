namespace TorrentClient;

public static class FileWorker
{
    public static byte[][] SplitFileIntoBlocks(FileMetaData fileMetaData, int blockSize)
    {
        long fileSize = new FileInfo(fileMetaData.FilePath).Length;
        int totalBlocks = (int)Math.Ceiling((double)fileSize / blockSize);
        
        byte[][] blocks = new byte[totalBlocks][];

        using FileStream fileStream = new FileStream(fileMetaData.FilePath, FileMode.Open, FileAccess.Read);

        for (int i = 0; i < totalBlocks; i++)
        {
            int currentBlockSize = (int)Math.Min(blockSize, fileSize - fileStream.Position);
            blocks[i] = new byte[currentBlockSize];
            fileStream.Read(blocks[i], 0, currentBlockSize);
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