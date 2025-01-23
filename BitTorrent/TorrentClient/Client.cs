using System.Collections.Concurrent;
using System.Net;
using System.Text;
using static TorrentClient.PackageHelper;

namespace TorrentClient;

public class Client
{
    private const int MaxTimeout = 5 * 60 * 1000;
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

    private async Task StartSharingFiles()
    {
        var buffer = new byte[MaxPacketSize];

        while (true)
        {
            var remoteEndPoint = await _networkClient.Receive(buffer, new IPEndPoint(IPAddress.Any, 0));

            var responseMessage = "PEER";
            await _networkClient.Send(remoteEndPoint, new PackageBuilder(10)
                .WithContent(Encoding.UTF8.GetBytes(responseMessage)));
        }
    }

    private async Task DownloadFiles()
    {
        var filesInProcess = _clientFiles
            .Where(f => f.FileStatus == FileStatus.Downloading);

        foreach (var fileMetaData in filesInProcess)
        {
            _fileProducers.TryAdd(fileMetaData.RootHash, await SearchPeers(fileMetaData));
        }
    }

    private async Task<List<ClientData>> SearchPeers(FileMetaData fileMetaData)
    {
        var requestMessage = Encoding.UTF8.GetBytes($"DISCOVER_PEERS WITH {fileMetaData.RootHash}");
        var buffer = new byte[MaxPacketSize];
        var peers = await _networkClient.SearchForPeers(buffer,
            new PackageBuilder(100)
                .WithContent(requestMessage), fileMetaData);

        return peers;
    }
}