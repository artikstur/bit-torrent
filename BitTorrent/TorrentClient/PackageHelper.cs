namespace TorrentClient;

public static class PackageHelper
{
    public const int MaxSizeOfContent = 244;
    public const int MaxPacketSize = 256;
    public const int MaxFreeBytes = MaxPacketSize - MaxSizeOfContent;
    
    public static byte[] GetContent(byte[] buffer, int contentLength) =>
        buffer.ToArray();
    
    public static byte[] CreatePackage(byte[] content) => 
        new PackageBuilder(content.Length)
            .WithContent(content)
            .Build();
}