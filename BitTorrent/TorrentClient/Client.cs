using System.Collections.Concurrent;
using System.Net;
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

        await Task.WhenAll(broadcastTask, clientTask);
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

            await _networkClient.Send(remoteEndPoint, new PackageBuilder(100)
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

            if (_fileProducers.TryGetValue(hash, out var clients))
            {
                Console.WriteLine($"Добавлен новый пир для файла с хэшем: {hash}");
                foreach (var client in clients)
                {
                    Console.WriteLine($"IP: {client.Ip}, Port: {client.Port}, Updated: {client.Updated}");
                }
            }
        }
    }

    private async Task DownloadFiles()
    {
        var filesInProcess = _clientFiles
            .Where(f => f.Value.FileStatus == FileStatus.Downloading);

        foreach (var fileMetaData in filesInProcess)
        {
            await SearchPeers(fileMetaData.Key, fileMetaData.Value);
        }
    }

    private async Task SearchPeers(string hash, FileMetaData fileMetaData)
    {
        await _networkClient.SearchForPeers(new PackageBuilder(100)
            .WithQuery(QueryType.Request)
            .WithPackageType(PackageType.Full)
            .WithCommand(CommandType.DiscoverPeers)
            .WithContent(Encoding.UTF8.GetBytes(hash.CreatePeerRequest())));
    }
}