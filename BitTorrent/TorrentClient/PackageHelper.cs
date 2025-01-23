namespace TorrentClient;

public static class PackageHelper
{
    public const int MaxSizeOfContent = 243;
    public const int MaxPacketSize = 256;
    public const int MaxFreeBytes = MaxPacketSize - MaxSizeOfContent;
    public const int PackageTypeIndex = 11;
    public const int QueryIndex = 10;

    public static readonly byte[] StartBytes = { 0x23, 0x23, 0x23 }; // # # # (3)
    public static readonly byte[] ProtocolBytes = { 0x54, 0x4F, 0x52, 0x52, 0x45, 0x4E, 0x54 }; // T O R R E N T (7)
    public static readonly byte EndByte = 0x03; // END OF TEXT (1)
    public static readonly byte SpaceByte = 0x20; // SPACE (1)

    public static readonly byte[] BasePackage =
    {
        0x23, 0x23, 0x23, 
        0x54, 0x4F, 0x52, 0x52, 0x45, 0x4E, 0x54
    };
    //           BASE PACKAGE + QUERY BYTE + PACKAGE TYPE + CONTENT + END BYTE 
    // EXAMPLE: "###TORRENT(query 1 byte)(full or not byte)(content many bytes)(end byte)"
}