using System.Net;
using System.Net.Sockets;
using System.Text;
using static TorrentClient.PackageHelper;

namespace TorrentClient;

public class Client
{
    private const int MaxTimeout = 5 * 60 * 1000;
    private readonly List<FileMetaData> _clientFiles;
    private readonly INetworkClient _networkClient;
    
    public Client(List<FileMetaData> clientFiles)
    {
        _clientFiles = clientFiles;
        _networkClient = new NetworkClient();
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
            try
            {
                while (true)
                {
                    // Заполнится данными, того кто постучался
                    var remoteEndPoint = await _networkClient.Receive(buffer, new IPEndPoint(IPAddress.Any, 0));
                    string receivedMessage = Encoding.UTF8.GetString(buffer);
                    
                    // Console.WriteLine($"{_clientPort} Получил сообщение: \"{receivedMessage}\" от {remoteEndPoint}");

                    string responseMessage = "PEER";
                    await _networkClient.Send(remoteEndPoint, new PackageBuilder(10)
                        .WithContent(Encoding.UTF8.GetBytes(responseMessage)));
                    // Console.WriteLine($"Ответ отправлен обратно к {remoteEndPoint}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при приеме сообщения: {ex.Message}");
            }
        }
    }
    
    private async Task DownloadFiles()
    {
        await ConnectToPeers();
    }

    private async Task ConnectToPeers()
    {
        var peers = await SearchPeers();
        // foreach (var peer in peers)
        // {
        //     try
        //     {
        //         await _senderSocket.ConnectAsync(peer.Ip, peer.Port);
        //     }
        //     catch (SocketException ex)
        //     {
        //         Console.WriteLine($"Не получилось подключиться к {peer.Ip} и {peer.Port}");
        //     }
        // }
    }

    private async Task<List<ClientData>> SearchPeers()
    {
        var requestMessage = Encoding.UTF8.GetBytes($"DISCOVER_PEERS");
        var buffer = new byte[MaxPacketSize];
        var peers = await 
            _networkClient.SearchForPeers(buffer, 
                new PackageBuilder(100)
                    .WithContent(requestMessage));
        return peers;
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