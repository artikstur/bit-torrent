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
    private readonly Dictionary<string, FileMetaData> _clientFiles = new();
    private readonly INetworkClient _networkClient = new NetworkClient();
    private readonly ConcurrentDictionary<string, List<ClientData>> _fileProducers = new();
    private CancellationTokenSource _cancellationTokenSource = new();
    
    public async Task Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        await Task.WhenAll(DownloadFiles(token), StartSharingFiles(token));
    }

    public async Task StopSharingFiles()
    {
        await _cancellationTokenSource.CancelAsync();
        await Task.Delay(100);
        Console.WriteLine("Файлы больше не расшариваются");
    }

    public async Task StopDownloading()
    {
        await _cancellationTokenSource.CancelAsync();
        await Task.Delay(100);
        Console.WriteLine("Загрузка завершена");
    }

    private async Task StartSharingFiles(CancellationToken token)
    {
        var sendBlocksTask = Task.Run(() => ProcessClientMessage(token), token);
        var broadcastTask = Task.Run(() => AnswerAsPeer(token), token);

        await Task.WhenAll(broadcastTask, sendBlocksTask);
    }

    private async Task ProcessClientMessage(CancellationToken token)
    {
        var buffer = new byte[MaxPacketSize];
        while (!token.IsCancellationRequested)
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

        Console.WriteLine("Обработка клиентских сообщений завершена.");
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

            // Console.WriteLine($"Добавлен новый пир для файла с хэшем: {hash}");
        });
    }

    private async Task AnswerAsPeer(CancellationToken token)
    {
        var buffer = new byte[MaxPacketSize];
        while (!token.IsCancellationRequested)
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

        Console.WriteLine("Ответы на запросы пиров завершены.");
    }

    private async Task DownloadFiles(CancellationToken token)
    {
        var filesInProcess = _clientFiles
            .Where(f => f.Value.FileStatus == FileStatus.Downloading)
            .ToDictionary(f => f.Key, f => f.Value);

        var searchPeersTask = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var fileMetaData in filesInProcess)
                {
                    await SearchPeers(fileMetaData.Key);
                }

                await Task.Delay(TimeSpan.FromSeconds(3), token);

                foreach (var fileMetaData in filesInProcess)
                {
                    await StartDownloadingWithRetries(fileMetaData.Value, token);
                }

                token.ThrowIfCancellationRequested();
            }
        }, token);
    }

    private async Task StartDownloadingWithRetries(FileMetaData fileMetaData, CancellationToken token)
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
        while (GetMissingBlocks(fileMetaData.Blocks).Count > 0 && !token.IsCancellationRequested)
        {
            await Task.Delay(retryInterval, token);
            await RetryMissingBlocks(fileMetaData, producers);
        }

        if (GetMissingBlocks(fileMetaData.Blocks).Count == 0 && fileMetaData.FileStatus == FileStatus.Downloading)
        {
            Console.WriteLine($"Загрузка завершена для файла {fileMetaData.FileName}");
            fileMetaData.FileStatus = FileStatus.Sharing;
            await FileWorker.WriteBlocksToFile(fileMetaData);
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
    
    public async Task AddFile(string rootHash, FileMetaData fileMetaData)
    {
        _clientFiles.TryAdd(rootHash, fileMetaData);
    }

    public async Task<List<FileMetaData>> GetFiles()
    {
        return _clientFiles.Select(file => file.Value).ToList();
    }
}