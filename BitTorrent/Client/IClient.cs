using System.Net.Sockets;
using Server;

namespace Client;

public interface IClient
{
    Task ConnectServerAsync();
    Task DisconnectServerAsync();
    Task ConnectPeerAsync(Socket peer);
    Task DisconnectPeerAsync(Socket peer);
    Task<List<Socket>> GetPeersForFile(FileMetaData fileMetaData);
    Task DownloadFileAsync(FileMetaData fileMetaData, string filePath);
    Task StartBeSeeder(FileMetaData fileMetaData);
    Task StopBeSeeder(FileMetaData fileMetaData);
    Task PushBlockData(FileMetaData fileMetaData, List<string> auditPath);
}