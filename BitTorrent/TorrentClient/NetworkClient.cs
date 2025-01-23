using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TorrentClient;

public class NetworkClient : INetworkClient
{
    private readonly Socket _broadcastListenerSocket;
    private readonly Socket _clientSocket;
    private readonly Socket _senderSocket;
    private readonly int _broadcastPort;
    private readonly int _clientPort;
    private readonly IPEndPoint _broadcastEndPoint;

    public NetworkClient(List<FileMetaData> clientFiles)
    {
        _broadcastPort = 23234;

        // Слушатель бродкастов
        _broadcastListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _broadcastListenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _broadcastListenerSocket.Bind(new IPEndPoint(IPAddress.Any, _broadcastPort));

        // Сокет под клиента
        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _clientSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
        _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
        _clientPort = ((IPEndPoint)_clientSocket.LocalEndPoint!).Port;
        
        _broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, _broadcastPort);
    }

    public async Task<EndPoint?> ReceiveBroadcast(byte[] buffer)
    {
        var result = await _broadcastListenerSocket.ReceiveFromAsync(buffer, new IPEndPoint(IPAddress.Any, 0));
        return IsSelf(result.RemoteEndPoint) ? null : result.RemoteEndPoint;
    }

    public async Task<EndPoint> ReceiveClientMessage(byte[] buffer)
    {
        var result = await _clientSocket.ReceiveFromAsync(buffer, new IPEndPoint(IPAddress.Any, 0));
        return result.RemoteEndPoint;
    }

    public async Task Send(EndPoint endPoint, PackageBuilder packageBuilder)
    {
        await _clientSocket.SendToAsync(packageBuilder.Build(), endPoint);
    }

    public async Task SearchForPeers(PackageBuilder packageBuilder)
    {
        await Send(_broadcastEndPoint, packageBuilder);
    }
    
    private bool IsSelf(EndPoint remoteEndPoint)
    {
        var remoteIpEndPoint = (IPEndPoint)remoteEndPoint;
        
        return remoteIpEndPoint.Port == _clientPort;
    }
}
