using System.Text;
using System.Text.Json;

namespace TorrentClient;

public static class PackageHelper
{
    public const int MaxSizeOfContent = 4096;
    public const int MaxPacketSize = 4110;
    public const int MaxFreeBytes = MaxPacketSize - MaxSizeOfContent;
    public const int CommandIndex = 12;
    public const int PackageTypeIndex = 11;
    public const int QueryIndex = 10;

    public static readonly byte[] StartBytes = { 0x23, 0x23, 0x23 }; // # # # (3)
    public static readonly byte[] ProtocolBytes = { 0x54, 0x4F, 0x52, 0x52, 0x45, 0x4E, 0x54 }; // T O R R E N T (7)
    public static readonly byte EndByte = 0x03; // END OF TEXT (1)
    public static readonly byte SpaceByte = 0x20; // SPACE (1)

    //           BASE PACKAGE + QUERY BYTE + PACKAGE TYPE + COMMAND TYPE + CONTENT + END BYTE 
    // EXAMPLE: "###TORRENT(query 1 byte)(full or not byte)(command)(content many bytes)(end byte)"
    public static readonly byte[] BasePackage =
    {
        0x23, 0x23, 0x23,
        0x54, 0x4F, 0x52, 0x52, 0x45, 0x4E, 0x54
    };

    public static bool IsDiscoverPeers(this byte[] buffer)
    {
        return buffer[CommandIndex] == (byte)CommandType.DiscoverPeers;
    }
    
    public static bool IsNeedBlock(this byte[] buffer)
    {
        return buffer[CommandIndex] == (byte)CommandType.NeedBlock;
    }

    public static bool IsBePeer(this byte[] buffer)
    {
        return buffer[CommandIndex] == (byte)CommandType.BePeer;
    }
    
    public static bool IsGiveBlock(this byte[] buffer)
    {
        return buffer[CommandIndex] == (byte)CommandType.GiveBlock;
    }

    public static BlockPacketRequest GetBlockPacketRequest(this byte[] buffer)
    {
        var message = Encoding.UTF8.GetString(buffer).Trim('\0');

        var jsonStartIndex = message.IndexOf('{');
        if (jsonStartIndex == -1)
            throw new ArgumentException("Неверный формат");

        var jsonPart = message.Substring(jsonStartIndex);

        var jsonEndIndex = jsonPart.LastIndexOf('}');
        if (jsonEndIndex == -1)
            throw new ArgumentException("Неверный формат");

        jsonPart = jsonPart.Substring(0, jsonEndIndex + 1);

        return JsonSerializer.Deserialize<BlockPacketRequest>(jsonPart) 
               ?? throw new ArgumentException("Неверный формат");
    }
    
    public static BlockPacketResponse GetBlockPacketResponse(this byte[] buffer)
    {
        var message = Encoding.UTF8.GetString(buffer).Trim('\0');

        var jsonStartIndex = message.IndexOf('{');
        if (jsonStartIndex == -1)
            throw new ArgumentException("Неверный формат");

        var jsonPart = message.Substring(jsonStartIndex);

        var jsonEndIndex = jsonPart.LastIndexOf('}');
        if (jsonEndIndex == -1)
            throw new ArgumentException("Неверный формат");

        jsonPart = jsonPart.Substring(0, jsonEndIndex + 1);

        return JsonSerializer.Deserialize<BlockPacketResponse>(jsonPart) 
               ?? throw new ArgumentException("Неверный формат");
    }
    
    public static string GetRootHashFromResponse(this byte[] buffer)
    {
        var message = Encoding.UTF8.GetString(buffer).Trim('\0');

        var jsonStartIndex = message.IndexOf('{');
        if (jsonStartIndex == -1)
            throw new ArgumentException("Неверный формат");

        var jsonPart = message.Substring(jsonStartIndex);

        var jsonEndIndex = jsonPart.LastIndexOf('}');
        if (jsonEndIndex == -1)
            throw new ArgumentException("Неверный формат");

        jsonPart = jsonPart.Substring(0, jsonEndIndex + 1);

        var peerMessage = JsonSerializer.Deserialize<PeerMessage>(jsonPart);
        return peerMessage?.Hash ?? throw new ArgumentException("Неверный формат");
    }

    public static string GetRootHashFromRequest(this byte[] buffer)
    {
        var message = Encoding.UTF8.GetString(buffer).Trim('\0');

        var jsonStartIndex = message.IndexOf('{');
        if (jsonStartIndex == -1)
            throw new ArgumentException("Неверный формат");

        var jsonPart = message.Substring(jsonStartIndex);

        var jsonEndIndex = jsonPart.LastIndexOf('}');
        if (jsonEndIndex == -1)
            throw new ArgumentException("Неверный формат");

        jsonPart = jsonPart.Substring(0, jsonEndIndex + 1);

        var peerMessage = JsonSerializer.Deserialize<PeerMessage>(jsonPart);
        return peerMessage?.Hash ?? throw new ArgumentException("Неверный формат");
    }

    public static bool IsResponse(this byte[] buffer)
    {
        return buffer[QueryIndex] == (byte)QueryType.Response;
    }

    public static bool IsRequest(this byte[] buffer)
    {
        return buffer[QueryIndex] == (byte)QueryType.Request;
    }

    public static bool IsFull(this byte[] buffer)
    {
        return buffer[PackageTypeIndex] == (byte)PackageType.Full;
    }

    public static bool IsPeerAnswer(this byte[] buffer)
    {
        return buffer.IsResponse()
               && buffer.IsBePeer()
               && buffer.IsFull();
    }
}