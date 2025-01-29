namespace TorrentClient;

public static class FileWorker
{
    public static byte[][] SplitFileIntoBlocks(string filePath, int blockSize)
    {
        long fileSize = new FileInfo(filePath).Length;
        int totalBlocks = (int)Math.Ceiling((double)fileSize / blockSize);
        
        byte[][] blocks = new byte[totalBlocks][];

        using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        for (int i = 0; i < totalBlocks; i++)
        {
            int currentBlockSize = (int)Math.Min(blockSize, fileSize - fileStream.Position);
            blocks[i] = new byte[currentBlockSize];
            fileStream.Read(blocks[i], 0, currentBlockSize);
        }

        return blocks;
    }
    
    public static long GetFileSize(string filePath)
    {
        if (File.Exists(filePath))
        {
            return new FileInfo(filePath).Length;
        }
        
        throw new FileNotFoundException("Файл не найден", filePath);
    }
    
    public static async Task WriteBlocksToFile(FileMetaData fileMetaData)
    {
        if (fileMetaData.Blocks == null || fileMetaData.Blocks.Length == 0)
        {
            throw new InvalidOperationException("Нет блоков для записи в файл.");
        }

        string directory = Path.GetDirectoryName(fileMetaData.FilePath) ?? "";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using FileStream fileStream = new FileStream(fileMetaData.FilePath, FileMode.Create, FileAccess.Write);

        foreach (var block in fileMetaData.Blocks)
        {
            fileStream.Write(block, 0, block.Length);
        }

        Console.WriteLine($"Файл успешно записан по пути: {fileMetaData.FilePath}");
    }
}