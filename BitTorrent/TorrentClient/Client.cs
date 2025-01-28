using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.JavaScript;
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
        var sendBlocksTask = Task.Run(ProcessClientMessage);
        var broadcastTask = Task.Run(AnswerAsPeer);

        await Task.WhenAll(broadcastTask, sendBlocksTask);
    }

    private async Task ProcessClientMessage()
    {
        var buffer = new byte[MaxPacketSize];
        while (true)
        {
            var remoteEndPoint = await _networkClient.ReceiveClientMessage(buffer);
            if (remoteEndPoint is null) continue;

            if (buffer.IsNeedBlock())
            {
                await HandleNeedBlockMessage(buffer, remoteEndPoint);
                continue;
            }

            if (buffer.IsGiveBlock())
            {
                HandleGiveBlockMessage(buffer);
                continue;
            }

            if (buffer.IsBePeer())
            {
                HandleBePeerMessage(buffer, remoteEndPoint);
            }
        }
    }

    private async Task HandleNeedBlockMessage(byte[] buffer, EndPoint remoteEndPoint)
    {
        var request = buffer.GetBlockPacketRequest();
        _ = Task.Run(async () =>
        {
            if (!_clientFiles.TryGetValue(request.Hash, out var fileData)
                || fileData.FileStatus != FileStatus.Sharing) return;

            await _networkClient.Send(remoteEndPoint, new PackageBuilder(MaxSizeOfContent)
                .WithQuery(QueryType.Response)
                .WithCommand(CommandType.GiveBlock)
                .WithPackageType(PackageType.Partial)
                .WithContent(Encoding.UTF8.GetBytes(request.Hash.CreateGivePacketResponse(
                    request.BlockIndex, fileData.Blocks[request.BlockIndex]))));

            Console.WriteLine($"Я отправил блок номер {request.BlockIndex}");
        });
    }

    private void HandleGiveBlockMessage(byte[] buffer)
    {
        var request = buffer.GetBlockPacketResponse();
        _ = Task.Run(() =>
        {
            Console.WriteLine($"Мне обратно пришел блок с номером {request.BlockIndex}");
            if (_clientFiles.TryGetValue(request.Hash, out var fileData) &&
                fileData.FileStatus == FileStatus.Downloading)
            {
                lock (fileData.Blocks)
                {
                    fileData.Blocks[request.BlockIndex] = request.Block;
                }
            }
        });
    }

    private void HandleBePeerMessage(byte[] buffer, EndPoint remoteEndPoint)
    {
        _ = Task.Run(() =>
        {
            var hash = buffer.GetRootHashFromRequest();

            var clientData = new ClientData
            {
                Ip = ((IPEndPoint)remoteEndPoint).Address,
                Port = ((IPEndPoint)remoteEndPoint).Port,
                Updated = DateTime.UtcNow
            };

            _fileProducers.AddOrUpdate(
                hash, _ => new List<ClientData> { clientData },
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

            Console.WriteLine($"Добавлен новый пир для файла с хэшем: {hash}");
        });
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

    private async Task DownloadFiles()
    {
        var filesInProcess = _clientFiles
            .Where(f => f.Value.FileStatus == FileStatus.Downloading);

        foreach (var fileMetaData in filesInProcess)
        {
            SearchPeers(fileMetaData.Key);
        }

        await Task.Delay(1000);
        foreach (var fileMetaData in filesInProcess)
        {
            StartDownloadingWithRetries(fileMetaData.Value);
        }
    }

    private List<int> GetMissingBlocks(byte[][] blocks)
    {
        var missingBlocks = new List<int>();
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] == null)
            {
                missingBlocks.Add(i);
            }
        }

        return missingBlocks;
    }

    private async Task RetryMissingBlocks(FileMetaData fileMetaData, List<ClientData> producers)
    {
        var missingBlocks = GetMissingBlocks(fileMetaData.Blocks);

        if (missingBlocks.Count == 0)
        {
            Console.WriteLine("Все блоки загружены.");
            return;
        }

        Console.WriteLine($"Повторный запрос {missingBlocks.Count} недостающих блоков.");

        foreach (var producer in producers)
        {
            foreach (var blockIndex in missingBlocks)
            {
                Console.WriteLine($"Запрашиваю блок {blockIndex} у пира {producer.Ip}:{producer.Port}");
                await AskBlockFromProducer(producer, fileMetaData, blockIndex);
            }
        }
    }

    private async Task StartDownloadingWithRetries(FileMetaData fileMetaData)
    {
        if (!_fileProducers.TryGetValue(fileMetaData.RootHash, out var producers))
        {
            Console.WriteLine("Нет раздающих для файла.");
            return;
        }

        foreach (var producer in producers)
        {
            for (int blockIndex = 0; blockIndex < fileMetaData.TotalBlocks; blockIndex++)
            {
                if (fileMetaData.Blocks[blockIndex] != null) continue;

                await AskBlockFromProducer(producer, fileMetaData, blockIndex);
            }
        }

        var retryInterval = TimeSpan.FromSeconds(5);
        while (GetMissingBlocks(fileMetaData.Blocks).Count > 0)
        {
            await Task.Delay(retryInterval);
            await RetryMissingBlocks(fileMetaData, producers);
        }

        Console.WriteLine($"Загрузка завершена для файла {fileMetaData.FileName}");
        await FileWorker.WriteBlocksToFile(fileMetaData);
    }

    private async Task AskBlockFromProducer(ClientData producer, FileMetaData fileMetaData, int blockIndex)
    {
        await _networkClient.Send(new IPEndPoint(producer.Ip, producer.Port),
            new PackageBuilder(MaxSizeOfContent)
                .WithQuery(QueryType.Request)
                .WithPackageType(PackageType.Full)
                .WithCommand(CommandType.NeedBlock)
                .WithContent(Encoding.UTF8.GetBytes(fileMetaData.RootHash.CreateNeedBlockRequest(blockIndex))));

        Console.WriteLine($"Я запросил у пира блок с индексом {blockIndex}");
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