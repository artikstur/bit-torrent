using System.Net.Sockets;

namespace Server;

public interface IServer
{
    Task StartAsync();
    Task StopAsync();
    Task ProcessSocketConnect(Socket socket);
    Task<List<Socket>> GetFilePeers(FileMetaData fileMetaData);
}