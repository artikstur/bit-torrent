using System.Collections.Concurrent;
using System.Net;
using System.Text;
using static TorrentClient.PackageHelper;

namespace TorrentClient;

public class Client
{
    private readonly List<FileMetaData> _clientFiles;
    private readonly INetworkClient _networkClient;
    private readonly ConcurrentDictionary<string, List<ClientData>> _fileProducers = new();

    public Client(List<FileMetaData> clientFiles)
    {
        _clientFiles = clientFiles;
        _networkClient = new NetworkClient(clientFiles);
    }

    public async Task Start()
    {
        await Task.WhenAll(DownloadFiles(), StartSharingFiles());
    }
    
    // TODO: добавить валидацию на пакеты
    // TODO: решить проблему стейтов у файлов
    // TODO: разобраться с локом
    private async Task StartSharingFiles()
    {
        var broadcastTask = Task.Run(async () =>
        {
            var buffer = new byte[MaxPacketSize];
            while (true)
            {
                var remoteEndPoint = await _networkClient.ReceiveBroadcast(buffer);

                if (remoteEndPoint is null) continue;
                if (!buffer.IsDiscoverPeers()) continue;

                var message = Encoding.UTF8.GetString(buffer).Trim('\0');
                var hash = message.Substring(CommandIndex + 1);
                hash = hash.Trim('\x03', '\0', ' ');

                if (_clientFiles.All(f => f.RootHash != hash)) continue;

                var responseMessage = $"I AM PEER FOR {hash}";
                await _networkClient.Send(remoteEndPoint, new PackageBuilder(100)
                    .WithQuery(QueryType.Response)
                    .WithCommand(CommandType.BePeer)
                    .WithPackageType(PackageType.Full)
                    .WithContent(Encoding.UTF8.GetBytes(responseMessage)));
            }
        });

        var clientTask = Task.Run(async () =>
        {
            var buffer = new byte[MaxPacketSize];
            while (true)
            {
                var remoteEndPoint = await _networkClient.ReceiveClientMessage(buffer);
                Console.WriteLine(Encoding.UTF8.GetString(buffer));

                var message = Encoding.UTF8.GetString(buffer).Trim('\0');

                var hash = message.Substring("###TORRENT!FBI AM PEER FOR ".Length).Trim('\x03', '\0', ' ');

                var clientData = new ClientData
                {
                    Ip = ((IPEndPoint)remoteEndPoint).Address,
                    Port = ((IPEndPoint)remoteEndPoint).Port,
                    Updated = DateTime.UtcNow
                };
                
                _fileProducers.AddOrUpdate(
                    hash, _ => [clientData],
                    (_, existingClients) =>
                    {
                        lock (existingClients)
                        {
                            var existingClient = existingClients.FirstOrDefault(c =>
                                c.Ip.Equals(clientData.Ip) && c.Port == clientData.Port);

                            if (existingClient != null)
                            {
                                existingClient.Updated = DateTime.UtcNow;
                            }
                            else
                            {
                                existingClients.Add(clientData);
                            }
                        }

                        return existingClients;
                    });
                
                if (_fileProducers.TryGetValue(hash, out var clients))
                {
                    Console.WriteLine($"Добавлен новый пир для файла с хэшем: {hash}");
                    foreach (var client in clients)
                    {
                        Console.WriteLine($"IP: {client.Ip}, Port: {client.Port}, Updated: {client.Updated}");
                    }
                }
            }
        });

        await Task.WhenAll(broadcastTask, clientTask);
    }

    private async Task DownloadFiles()
    {
        var filesInProcess = _clientFiles
            .Where(f => f.FileStatus == FileStatus.Downloading);

        foreach (var fileMetaData in filesInProcess)
        {
            await SearchPeers(fileMetaData);
        }
    }

    private async Task SearchPeers(FileMetaData fileMetaData)
    {
        var requestMessage = Encoding.UTF8.GetBytes($"{fileMetaData.RootHash}");
        await _networkClient.SearchForPeers(new PackageBuilder(100)
            .WithQuery(QueryType.Request)
            .WithPackageType(PackageType.Full)
            .WithCommand(CommandType.DiscoverPeers)
            .WithContent(requestMessage));
    }
}