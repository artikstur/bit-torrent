namespace TorrentClient;

public static class FileWorker
{
    public static List<byte[]> SplitFileIntoBlocks(FileMetaData fileMetaData)
    {
        var bytesParts = new List<byte[]>();

        using var fileStream = new FileStream(fileMetaData.FilePath, FileMode.Open, FileAccess.Read);
        var buffer = new byte[fileMetaData.BlockSize];

        while (true)
        {
            int bytesRead = fileStream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0) break;
            
            var block = bytesRead < buffer.Length
                ? buffer.Take(bytesRead).ToArray()
                : (byte[])buffer.Clone();

            bytesParts.Add(block);
        }

        return bytesParts;
    }
}