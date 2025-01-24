using TorrentClient;

var clients = new List<Client>();
var client1 = new Client(new Dictionary<string, FileMetaData>
{
    { "2", new FileMetaData {FileStatus = FileStatus.Downloading}}
});

var client2 = new Client(new Dictionary<string, FileMetaData>
{
    { "2", new FileMetaData {FileStatus = FileStatus.Sharing}}
});

var client3 = new Client(new Dictionary<string, FileMetaData>
{
    { "2", new FileMetaData {FileStatus = FileStatus.Sharing}}
});

clients.Add(client1);
clients.Add(client2);
clients.Add(client3);

var tasks = clients.Select(client => Task.Run(async () => await client.Start())).ToArray();

await Task.WhenAll(tasks);