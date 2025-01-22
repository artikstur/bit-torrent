using System.Net;
using System.Net.Sockets;
using System.Text;
using static TorrentClient.PackageHelper;

namespace TorrentClient;

public class Client
{
    private readonly Socket _listenerSocket;
    private const int MaxTimeout = 5 * 60 * 1000;
    private const int BroadcastPort = 6000;
    private readonly int _clientPort;
    private readonly List<FileMetaData> _clientFiles;

    // На вход принимает файлы, которые у него уже готовы под раздачу и файлы, которые он еще должен загрузить
    // TODO: Настроить выход клиента в сеть
    public Client(List<FileMetaData> clientFiles, int clientPort)
    {
        _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _clientPort = clientPort;
        _listenerSocket.Bind(new IPEndPoint(IPAddress.Any, _clientPort));
        _listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
    }

    public async Task Start()
    {
        _ = Task.Run(StartSharingFiles);
        _ = Task.Run(DownloadFiles);
    }

    private async Task StartSharingFiles()
    {
        var buffer = new byte[MaxPacketSize];

        while (true)
        {
            try
            {
                var result = await _listenerSocket.ReceiveFromAsync(new ArraySegment<byte>(buffer),
                    SocketFlags.None, new IPEndPoint(IPAddress.Any, BroadcastPort));
                Console.WriteLine($"Получено сообщение");

                if (result.RemoteEndPoint is not IPEndPoint remoteEndpoint) continue;

                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.ReceivedBytes);
                Console.WriteLine($"Получено сообщение от {remoteEndpoint}: {receivedMessage}");

                await HandleIncomingMessage(receivedMessage, remoteEndpoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при приеме сообщения: {ex.Message}");
            }
        }
    }

    private async Task HandleIncomingMessage(string message, IPEndPoint remoteEndpoint)
    {
        if (message.StartsWith("DISCOVER_PEERS"))
        {
            // Формируем ответ, например, с информацией о текущем пире
            var responseMessage = $"PEER_INFO:{_clientPort}";
            var responseData = Encoding.UTF8.GetBytes(responseMessage);

            await _listenerSocket.SendToAsync(new ArraySegment<byte>(responseData), SocketFlags.None, remoteEndpoint);
            Console.WriteLine($"Ответ отправлен на {remoteEndpoint}: {responseMessage}");
        }
    }

    public void Stop()
    {
        _listenerSocket.Close();
    }

    // TODO: Продумать как клиентов будет искать других в сети и как отбирать нужные файлы
    private async Task DownloadFiles()
    {
        var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        await ConnectToPeers(udpSocket);
        var name = $"PEER_INFO:{_clientPort}";
        var content = Encoding.UTF8.GetBytes(name);
        var helloPackage = CreatePackage(content);

        await udpSocket.SendAsync(new ArraySegment<byte>(helloPackage), SocketFlags.None);
    }

    // Подключаемся ко всем пирам
    private async Task ConnectToPeers(Socket udpSocket)
    {
        var peers = await SearchPeers(udpSocket);
        foreach (var peer in peers)
        {
            try
            {
                await udpSocket.ConnectAsync(peer.Ip, peer.Port);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Не получилось подключиться к {peer.Ip} и {peer.Port}");
            }
        }
    }

    // Находит всех пиров в сети
    private async Task<List<ClientData>> SearchPeers(Socket udpSocket)
    {
        var peers = new List<ClientData>();
        udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

        var broadcastEndpoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), BroadcastPort);

        var requestMessage = Encoding.UTF8.GetBytes("DISCOVER_PEERS");
        await udpSocket.SendToAsync(new ArraySegment<byte>(requestMessage), SocketFlags.None, broadcastEndpoint);

        var buffer = new byte[MaxPacketSize];

        try
        {
            while (true)
            {
                var result = await udpSocket.ReceiveMessageFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None,
                    new IPEndPoint(IPAddress.Any, 0));
                var responseMessage = Encoding.UTF8.GetString(buffer, 0, result.ReceivedBytes);

                var peerData = ParsePeerData(responseMessage, (IPEndPoint)result.RemoteEndPoint);
                if (peerData == null || peers.Any(p => p.Ip.Equals(peerData.Ip) && p.Port == peerData.Port))
                    continue;

                peers.Add(peerData);
                Console.WriteLine($"Найден пир: {peerData.Ip}:{peerData.Port}");
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Ошибка приема данных: {ex.Message}");
        }

        return peers;
    }

    private ClientData? ParsePeerData(string message, IPEndPoint remoteEndpoint)
    {
        if (!message.StartsWith("PEER_INFO:")) return null;

        var info = message.Substring("PEER_INFO:".Length).Split(',');
        if (info.Length == 2 && int.TryParse(info[1], out var port))
        {
            return new ClientData
            {
                Ip = remoteEndpoint.Address,
                Port = port
            };
        }

        return null;
    }

    // Находит всех пиров, раздающих конкретный файл, в сети
    private Task<List<Socket>> SearchPeersForFile(FileMetaData fileMetaData)
    {
        throw new NotImplementedException();
    }

    private Task DownloadFileAsync(FileMetaData fileMetaData, string filePath)
    {
        throw new NotImplementedException();
    }

    // Стать раздающим пиром для конкретного файла
    private Task StartBeSeeder(FileMetaData fileMetaData)
    {
        throw new NotImplementedException();
    }

    // Перестать быть раздающим пиром для конкретного файла
    private Task StopBeSeeder(FileMetaData fileMetaData)
    {
        throw new NotImplementedException();
    }

    // Отправить блок данных
    private Task PushBlockData(FileMetaData fileMetaData, List<string> auditPath)
    {
        throw new NotImplementedException();
    }

    // Чтобы потом во время жизни объекта добавлять файлы, которые юзер скачал и готов раздавать
    public void AddFileForDistribution(FileMetaData file)
    {
        _clientFiles.Add(file);
    }
}