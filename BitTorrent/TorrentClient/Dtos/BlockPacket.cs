namespace TorrentClient;

public class BlockPacketRequest
{
    public string Hash { get; set; }
    public int BlockIndex { get; set; }
}