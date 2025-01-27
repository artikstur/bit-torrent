using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using TorrentClient.Extensions;
using static TorrentClient.PackageHelper;

namespace TorrentClient;

public class Client
{
    private readonly Dictionary<string, FileMetaData> _clientFiles;
    private readonly INetworkClient _networkClient;
    private readonly ConcurrentDictionary<string, List<ClientData>> _fileProducers = new();

    public Client(Dictionary<string, FileMetaData> clientFiles)
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
        var broadcastTask = Task.Run(AnswerAsPeer);
        var clientTask = Task.Run(DeterminePeer);
        var sendBlocksTask = Task.Run(SendBlocks);

        await Task.WhenAll(broadcastTask, clientTask, sendBlocksTask);
    }
    
    // TODO: Добавить auditpath в ответ вместе с блоком
    private async Task SendBlocks()
    {
        var buffer = new byte[MaxPacketSize];
        while (true)
        {
            var remoteEndPoint = await _networkClient.ReceiveClientMessage(buffer);
            if (remoteEndPoint is null) continue;
            
            var a = Encoding.UTF8.GetString(buffer);
            if (!buffer.IsNeedBlock()) continue;
            var request = buffer.GetBlockPacketRequest();

            if (!_clientFiles.TryGetValue(request.Hash, out var fileData)
                || fileData.FileStatus != FileStatus.Sharing) return;

            await _networkClient.Send(remoteEndPoint, new PackageBuilder(MaxSizeOfContent)
                .WithQuery(QueryType.Response)
                .WithCommand(CommandType.GiveBlock)
                .WithPackageType(PackageType.Partial)
                .WithContent(fileData.Blocks[request.BlockIndex]));
        }
    }

    private async Task AnswerAsPeer()
    {
        var buffer = new byte[MaxPacketSize];
        while (true)
        {
            var remoteEndPoint = await _networkClient.ReceiveBroadcast(buffer);
            if (remoteEndPoint is null) continue;

            if (!buffer.IsDiscoverPeers()) continue;
            var hash = buffer.GetRootHashFromRequest();
            if (!_clientFiles.TryGetValue(hash, out var file)) continue;

            await _networkClient.Send(remoteEndPoint, new PackageBuilder(MaxSizeOfContent)
                .WithQuery(QueryType.Response)
                .WithCommand(CommandType.BePeer)
                .WithPackageType(PackageType.Full)
                .WithContent(Encoding.UTF8.GetBytes(hash.CreatePeerAnswer())));
        }
    }

    private async Task DeterminePeer()
    {
        var buffer = new byte[MaxPacketSize];
        while (true)
        {
            var remoteEndPoint = await _networkClient.ReceiveClientMessage(buffer);

            if (!buffer.IsBePeer()) return;
            var hash = buffer.GetRootHashFromRequest();

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

            if (!_fileProducers.TryGetValue(hash, out var clients)) continue;

            Console.WriteLine($"Добавлен новый пир для файла с хэшем: {hash}");
            foreach (var client in clients)
            {
                Console.WriteLine($"IP: {client.Ip}, Port: {client.Port}, Updated: {client.Updated}");
            }
        }
    }
    
    // TODO: разобраться с Delay
    private async Task DownloadFiles()
    {
        var filesInProcess = _clientFiles
            .Where(f => f.Value.FileStatus == FileStatus.Downloading);

        foreach (var fileMetaData in filesInProcess)
        {
            await SearchPeers(fileMetaData.Key);
        }

        await Task.Delay(3000);
        foreach (var fileMetaData in filesInProcess)
        {
            await StartDownloading(fileMetaData.Value);
        }
    }

    // TODO: Настроить прием данных от пиров в общий массив
    // TODO: Валидировать данные
    private async Task StartDownloading(FileMetaData fileMetaData)
    {
        var blocks = new byte[fileMetaData.TotalBlocks][];

        if (!_fileProducers.TryGetValue(fileMetaData.RootHash, out var producers))
        {
            Console.WriteLine("No producers.");
            return;
        }

        var downloadTasks = new List<Task>();

        foreach (var producer in producers)
        {
            for (int blockIndex = 0; blockIndex < fileMetaData.TotalBlocks - 1; blockIndex++)
            {
                if (blocks[blockIndex] != null) continue;

                downloadTasks.Add(Task.Run(async () =>
                {
                    GetBlockFromProducer(producer, fileMetaData, blockIndex);
                }));
            }
        }

        await Task.WhenAll(downloadTasks);

        Console.WriteLine("Еее я все скачал");
    }

    private async Task GetBlockFromProducer(ClientData producer, FileMetaData fileMetaData, int blockIndex)
    {
        await _networkClient.Send(new IPEndPoint(producer.Ip, producer.Port),
            new PackageBuilder(MaxSizeOfContent)
                .WithQuery(QueryType.Request)
                .WithPackageType(PackageType.Full)
                .WithCommand(CommandType.NeedBlock)
                .WithContent(Encoding.UTF8.GetBytes(fileMetaData.RootHash.CreateNeedBlockRequest(blockIndex))));
    }

    private async Task SearchPeers(string hash)
    {
        await _networkClient.SearchForPeers(new PackageBuilder(MaxSizeOfContent)
            .WithQuery(QueryType.Request)
            .WithPackageType(PackageType.Full)
            .WithCommand(CommandType.DiscoverPeers)
            .WithContent(Encoding.UTF8.GetBytes(hash.CreatePeerRequest())));
    }
}