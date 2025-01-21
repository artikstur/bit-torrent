using System.Net.Sockets;
using Server;

namespace Client;

public class Client
{
    private readonly Socket _socket;
    private const int MaxTimeout = 5 * 60 * 1000;
    private readonly List<FileMetaData> _distributionFiles;
    private readonly List<DownloadingFile> _downloadingFiles;

    // На вход принимает файлы, которые у него уже готовы под раздачу и файлы, которые он еще должен загрузить
    public Client(List<FileMetaData> distributionFiles, List<DownloadingFile> downloadingFiles)
    {
        _distributionFiles = distributionFiles;
        _downloadingFiles = downloadingFiles;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Udp);
    }

    public async Task Start()
    {
        // Как только сервер запускается, он начинает две функции делать: раздавать файлы и загружать, что еще не загрузил
        _socket.Listen();
        _ = StartSharingFiles();
        _ = DownloadFiles();
    }
    
    private async Task StartSharingFiles()
    {
        try
        {
            while (true)
            {
                var cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(MaxTimeout);
                
                var connectionSocket = await _socket.AcceptAsync(cancellationToken.Token);
                var innerCancellationToken = new CancellationTokenSource();
                _ = Task.Run(
                    async () =>
                        await ProcessSocketConnect(connectionSocket, innerCancellationToken),
                    innerCancellationToken.Token);
            }
        }
        catch
        {
            Stop();
        }
    }

    public void Stop()
    {
        _socket.Close();
    }
    
    private Task ProcessSocketConnect(Socket socket, CancellationTokenSource ctxSource)
    {
        throw new NotImplementedException();
    }

    private Task DownloadFiles()
    {
        throw new NotImplementedException();
    }

    private Task<List<Socket>> SearchPeersForFile(FileMetaData fileMetaData)
    {
        throw new NotImplementedException();
    }
    
    private Task DownloadFileAsync(FileMetaData fileMetaData, string filePath)
    {
        throw new NotImplementedException();
    }

    private Task StartBeSeeder(FileMetaData fileMetaData)
    {
        throw new NotImplementedException();
    }

    private Task StopBeSeeder(FileMetaData fileMetaData)
    {
        throw new NotImplementedException();
    }

    private Task PushBlockData(FileMetaData fileMetaData, List<string> auditPath)
    {
        throw new NotImplementedException();
    }

    // Чтобы потом во время жизни объекта добавлять файлы, которые юзер скачал и готов раздавать
    public Task AddFileForDistribution()
    {
        throw new NotImplementedException();
    }
}