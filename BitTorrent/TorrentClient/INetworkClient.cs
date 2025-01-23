using System.Net;

namespace TorrentClient;

public interface INetworkClient
{
    Task<EndPoint> Receive(byte[] buffer, EndPoint endPoint);
    Task Send(EndPoint endPoint, PackageBuilder packageBuilder);
    Task<List<ClientData>> SearchForPeers(byte[] buffer, PackageBuilder packageBuilder);
}