namespace TorrentClient;

public enum CommandType: byte
{
    DiscoverPeers = 0x44,
    BePeer = 0x42,
}