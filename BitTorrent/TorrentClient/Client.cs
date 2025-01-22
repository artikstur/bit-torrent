using System.Net;
using System.Net.Sockets;
using System.Text;
using static TorrentClient.PackageHelper;

namespace TorrentClient;

public class Client
{
    private readonly Socket _listenerSocket;
    private readonly Socket _senderSocket;
    private const int MaxTimeout = 5 * 60 * 1000;
    private readonly int _broadCastPort;
    private readonly int _clientPort;
    private readonly List<FileMetaData> _clientFiles;

    // На вход принимает файлы, которые у него уже готовы под раздачу и файлы, которые он еще должен загрузить
    // TODO: Настроить выход клиента в сеть
    public Client(List<FileMetaData> clientFiles, int broadCastPort, int clientPort)
    {
        _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _broadCastPort = broadCastPort;
        _clientPort = clientPort;
        _listenerSocket.Bind(new IPEndPoint(IPAddress.Any, _broadCastPort));
        
        _senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _senderSocket.Bind(new IPEndPoint(IPAddress.Any, _clientPort));
    }
    
    public async Task Start()
    {
        await Task.WhenAll(DownloadFiles(), StartSharingFiles());
    }

    private async Task StartSharingFiles()
    {
        var buffer = new byte[MaxPacketSize];

        while (true)
        {
            // Отвечаем на запрос поиска пиров
            try
            {
                while (true)
                {
                    // Заполнится данными, того кто постучался
                    EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int receivedBytes = _listenerSocket.ReceiveFrom(buffer, ref senderEndPoint);
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    
                    // Console.WriteLine($"{_clientPort} Получил сообщение: \"{receivedMessage}\" от {senderEndPoint}");

                    string responseMessage = "PEER";
                    byte[] responseBuffer = Encoding.UTF8.GetBytes(responseMessage);

                    await _senderSocket.SendToAsync(responseBuffer, senderEndPoint);
                    // Console.WriteLine($"Ответ отправлен: \"{responseMessage}\" обратно к {senderEndPoint}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при приеме сообщения: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _listenerSocket.Close();
    }

    private async Task DownloadFiles()
    {
        await ConnectToPeers();
    }

    private async Task ConnectToPeers()
    {
        var peers = await SearchPeers();
        foreach (var peer in peers)
        {
            try
            {
                await _senderSocket.ConnectAsync(peer.Ip, peer.Port);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Не получилось подключиться к {peer.Ip} и {peer.Port}");
            }
        }
    }

    private async Task<List<ClientData>> SearchPeers()
    {
        // Пир отправляет в сеть запрос, чтобы найти всех других пиров
        var peers = new List<ClientData>();
        _senderSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
        var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, _broadCastPort);

        var requestMessage = Encoding.UTF8.GetBytes($"DISCOVER_PEERS FROM {_clientPort}");
        await _senderSocket.SendToAsync(new ArraySegment<byte>(requestMessage), broadcastEndpoint);

        var buffer = new byte[MaxPacketSize];

        // Здесь пир получает инфу от каждого пира о нем, если он подключен к сети и складывает в список
        try
        {
            while (true)
            {
                var peerMessage = await _senderSocket.ReceiveMessageFromAsync(new ArraySegment<byte>(buffer),
                    SocketFlags.None,
                    new IPEndPoint(IPAddress.Any, 0));
                var responseMessage = Encoding.UTF8.GetString(buffer, 0, peerMessage.ReceivedBytes);
        
                var peerData = ParsePeerData(responseMessage, (IPEndPoint)peerMessage.RemoteEndPoint);
                if (peerData == null)
                    continue;

                peers.Add(peerData);
                Console.WriteLine($"{_clientPort} нашел пира: {peerData.Ip}:{peerData.Port}");
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
        if (!message.StartsWith("PEER")) 
            return null;
        if (remoteEndpoint.Port == _clientPort)
            return null;
        
        return new ClientData
        {
            Ip = remoteEndpoint.Address,
            Port = remoteEndpoint.Port
        };
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