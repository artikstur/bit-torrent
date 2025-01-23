using System.Net;
using System.Net.Sockets;

namespace TorrentClient;

public class NetworkClient : INetworkClient
{
    private readonly Socket _listenerSocket;
    private readonly Socket _senderSocket;
    private readonly int _broadcastPort;
    private readonly int _clientPort;
    private readonly IPEndPoint _broadcastEndPoint;

    public NetworkClient()
    {
        _broadcastPort = 23234;
        _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _listenerSocket.Bind(new IPEndPoint(IPAddress.Any, _broadcastPort));

        _senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _senderSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
        _senderSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
        _clientPort = ((IPEndPoint)_senderSocket.LocalEndPoint!).Port;

        _broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, _broadcastPort);
    }

    public async Task<EndPoint> Receive(byte[] buffer, EndPoint endPoint)
    {
        var result = await _listenerSocket.ReceiveFromAsync(buffer, endPoint);
        return result.RemoteEndPoint;
    }

    public async Task Send(EndPoint endPoint, PackageBuilder packageBuilder)
    {
        ArraySegment<byte> responseBuffer = packageBuilder.Build();

        await _senderSocket.SendToAsync(responseBuffer, endPoint);
    }

    // TODO: Валидация ответа от пира. Нужно проверить какой он ответ вернул для безопасности
    public async Task<List<ClientData>> SearchForPeers(byte[] buffer, PackageBuilder packageBuilder)
    {
        var peers = new List<ClientData>();
        var cancellationTokenSource = new CancellationTokenSource();
        
        await Send(_broadcastEndPoint, packageBuilder);

        while (true)
        {
            var receiveTask = Receive(buffer, new IPEndPoint(IPAddress.Any, 0));
            var completedTask =
                await Task.WhenAny(receiveTask, Task.Delay(10000, cancellationTokenSource.Token));

            if (completedTask == receiveTask)
            {
                var result = (IPEndPoint)await receiveTask;
                if (result.Port == _clientPort) continue;

                peers.Add(new ClientData()
                {
                    Ip = result.Address,
                    Port = result.Port
                });

                Console.WriteLine($"{_clientPort} нашел {result.Port}");
            }
            else
            {
                break;
            }
        }
        
        Console.WriteLine("Поиск пиров завершен");
        return peers;
    }
}