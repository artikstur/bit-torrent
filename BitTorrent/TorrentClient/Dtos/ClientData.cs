using System.Net;

namespace TorrentClient;

public class ClientData
{
    public IPAddress Ip { get; set; }
    public int Port { get; set; }
    public DateTime Updated { get; set; }
}