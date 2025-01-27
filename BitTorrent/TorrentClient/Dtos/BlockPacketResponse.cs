namespace TorrentClient;

public class BlockPacketResponse
{
    public string Hash { get; set; }
    public int BlockIndex { get; set; }
    public byte[] Block { get; set; }
}