using TorrentClient;

var clients = new List<Client>();
var client1 = new Client(new List<FileMetaData>()
{
    new ()
    {
        RootHash = "2",
        FileStatus = FileStatus.Downloading
    }
});

var client2 = new Client(new List<FileMetaData>{
    new ()
    {
        RootHash = "2",
        FileStatus = FileStatus.Sharing
    },
    new ()
    {
        RootHash = "232332",
        FileStatus = FileStatus.Sharing
    }
});

var client3 = new Client(new List<FileMetaData>{
    new ()
    {
        RootHash = "2",
        FileStatus = FileStatus.Sharing
    }
});

clients.Add(client1);
clients.Add(client2);
clients.Add(client3);

var tasks = clients.Select(client => Task.Run(async () => await client.Start())).ToArray();

await Task.WhenAll(tasks);