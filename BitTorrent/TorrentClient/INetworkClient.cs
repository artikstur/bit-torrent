using System.Net;

namespace TorrentClient;

public interface INetworkClient
{
    Task<EndPoint?> ReceiveBroadcast(byte[] buffer);
    Task<EndPoint> ReceiveClientMessage(byte[] buffer);
    Task Send(EndPoint endPoint, PackageBuilder packageBuilder);
    Task SearchForPeers(PackageBuilder packageBuilder);
}